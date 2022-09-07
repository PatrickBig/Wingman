using WarThunder.Wingman.Utilities;
using Blazored.LocalStorage;
using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Numerics;
using System.Text.Json;

namespace WarThunder.Wingman.Shared
{
    public partial class CombatMap
    {
        private MapInformation? _mapInformation;
        private MapObject[]? _mapObjects;
        private Canvas? _canvas;
        private CombatMapOptions _mapOptions = new();

        private const double periodNormal = 2.0;
        private const double periodHeavy = 1.2;

        // Timer related things for tics/etc
        private Timer _refreshTimer = null!;
        private DateTime _currentTick;
        private DateTime? _lastTick;
        private double _delta = 0.0;
        private double _blinkNormalT = 0;
        private double _blinkHeavyT = 0;
        private double _blinkNormalValue = 0.0;
        private double _blinkHeavyValue = 0.0;

        private int _currentMapGen = -1;
        private int _lastMapGen = -2;

        // Map fields
        double _mapScale = 1.0;
        double[] _mapPan = { 0, 0 };
        Vector2? _lastPlayerPosition;
        private bool _isPanning = false;
        private double[] _previousMousePosition = new double[] { 0, 0 };

        
        [Parameter]
        public EventCallback<MapInformation> MapChanged { get; set; }

        [Inject]
        private HttpClient Http { get; set; } = null!;

        [Inject]
        private IOptions<WarThunderOptions> WarThunderOptions { get; set; } = null!;

        [Inject]
        private ILogger<CombatMap> Logger { get; set; } = null!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = null!;

        [Inject]
        ISyncLocalStorageService LocalStorage { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {

            // Initialize the map options
            var mapOptions = LocalStorage.GetItem<CombatMapOptions>("combat-map-settings");

            if (mapOptions == null)
            {
                mapOptions = new CombatMapOptions();
                LocalStorage.SetItem<CombatMapOptions>("combat-map-settings", mapOptions);
            }

            await base.OnInitializedAsync();
        }

        private double UpdateTimers()
        {
            _currentTick = DateTime.Now;
            _delta = 0.0;

            if (_lastTick.HasValue)
            {
                _delta = (_currentTick.Ticks - _lastTick.Value.Ticks) * 0.001;
                _blinkNormalT += _delta;
                _blinkHeavyT += _delta;


                if (_blinkNormalT > periodNormal)
                {
                    _blinkNormalT -= periodNormal * Math.Floor(_blinkNormalT / periodNormal);
                }

                if (_blinkHeavyT > periodHeavy)
                {
                    _blinkHeavyT -= periodHeavy * Math.Floor(_blinkHeavyT / periodHeavy);
                }

                _blinkNormalValue = Math.Exp(-Math.Pow(5 * _blinkNormalT - 2, 4));
                _blinkHeavyValue = Math.Exp(-Math.Pow(5 * _blinkHeavyT - 2, 4));

            }

            _lastTick = _currentTick;

            return _delta;
        }

        private async Task UpdateMapInformationAsync()
        {
            try
            {
                _mapInformation = await Http.GetFromJsonAsync<MapInformation>("map_info.json");


                if (_mapInformation != null)
                {
                    _currentMapGen = _mapInformation.MapGeneration;

                    if (_currentMapGen != _lastMapGen)
                    {
                        _lastMapGen = _currentMapGen;

                        // Reset zoom/scale.
                        _mapScale = 1.0;
                        _mapPan[0] = 0;
                        _mapPan[1] = 0;
                        await JSRuntime.InvokeVoidAsync("eval", $"mapImage.src = \"{(WarThunderOptions.Value.WarThunderEndpoint + MapHelpers.GetMapImageUrl(_mapInformation.MapGeneration))}\";");
                    }
                }
            }
            catch (JsonException ex)
            {
                _mapInformation = null;
                Logger.LogError("Failed to get map information", ex);
            }
        }

        private async Task UpdateMapObjectsAsync()
        {
            try
            {
                _mapObjects = await Http.GetFromJsonAsync<MapObject[]>("map_obj.json");

                var delta = UpdateTimers();
                if (!_isPanning)
                {
                    await RedrawMap(delta);
                }
            }
            catch (JsonException)
            {
                // The map info is no longer valid. Most likely changed maps.
                _mapObjects = Array.Empty<MapObject>();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            if (_canvas != null)
            {
                // Launch the timer
                _refreshTimer = new Timer(async _ =>
                {
                    await UpdateMapInformationAsync();
                    await UpdateMapObjectsAsync();
                }, null, 0, 500);

            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task OnZoom(WheelEventArgs e)
        {
            var delta = e.DeltaY * -1;
            delta *= _mapScale * 0.8;

            // Calculate offsets for zooming to a location
            var updatedMapScale = MathHelpers.Clamp(_mapScale + (delta * 0.001), 1.0, 15.0);
            _mapPan[0] = e.OffsetX - (e.OffsetX - _mapPan[0]) * updatedMapScale / _mapScale;
            _mapPan[1] = e.OffsetY - (e.OffsetY - _mapPan[1]) * updatedMapScale / _mapScale;
            _mapScale = updatedMapScale;

            ClampMapPan();

            await RedrawMap(0.0);
        }

        private Task MouseDown(MouseEventArgs e)
        {
            if (e.Button == 0)
            {
                _isPanning = true;
            }
            return Task.CompletedTask;
        }

        private Task MouseUp(MouseEventArgs e)
        {
            _isPanning = false;
            return Task.CompletedTask;
        }


        private async Task MouseMove(MouseEventArgs e)
        {
            // Check if the mouse button is down (we are dragging)
            if (_isPanning)
            {
                // Set the pan based on event offsets
                _mapPan[0] += e.PageX - _previousMousePosition[0];
                _mapPan[1] += e.PageY - _previousMousePosition[1];
                ClampMapPan();

                await RedrawMap(0.0);
            }

            // Record the mouse position for future drag events
            _previousMousePosition[0] = e.PageX;
            _previousMousePosition[1] = e.PageY;
        }

        private async Task RedrawMap(double delta)
        {
            if (_canvas != null)
            {
                await using var context = await _canvas.GetContext2DAsync();

                await context.ClearRectAsync(0, 0, _mapOptions.Width, _mapOptions.Height);

                await context.SetTransformAsync(_mapScale, 0, 0, _mapScale, _mapPan[0], _mapPan[1]);

                // Make sure the map image has loaded
                var mapComplete = await JSRuntime.InvokeAsync<bool>("isMapImageComplete");
                if (mapComplete)
                {
                    await context.DrawImageAsync("mapImage", 0, 0, _mapOptions.Width, _mapOptions.Height);
                }

                await context.SetTransformAsync(1, 0, 0, 1, 0, 0);

                await DrawMapGridAsync();
                await DrawMapObjectsAsync();
            }
        }

        private void ClampMapPan()
        {
            _mapPan[0] = MathHelpers.Clamp(_mapPan[0], -(_mapScale - 1.0) * _mapOptions.Width, 0);
            _mapPan[1] = MathHelpers.Clamp(_mapPan[1], -(_mapScale - 1.0) * _mapOptions.Height, 0);
        }

        private async Task DrawMapObjectsAsync()
        {
            if (_mapObjects != null && _mapObjects.Any() && _canvas != null)
            {
                await using var context = await _canvas.GetContext2DAsync();

                MapObject? player = null;
                foreach (var obj in _mapObjects)
                {
                    if (obj.Type == "airfield")
                    {
                        await DrawAirfieldAsync(context, obj);
                    }
                    else
                    {
                        if (obj.Icon == "Player")
                        {
                            player = obj;
                        }
                        else
                        {
                            // It's something else.
                            await DrawMapObjectAsync(context, obj);
                        }
                    }
                }

                if (player != null)
                {
                    await DrawPlayerAsync(context, player);
                }
            }
        }

        private async Task DrawAirfieldAsync(Context2D context, MapObject airField)
        {
            var sx = _mapOptions.Width * (airField.sx ?? 0) * _mapScale + _mapPan[0];
            var sy = _mapOptions.Height * (airField.sy ?? 0) * _mapScale + _mapPan[1];
            var ex = _mapOptions.Width * (airField.ex ?? 0) * _mapScale + _mapPan[0];
            var ey = _mapOptions.Height * (airField.ey ?? 0) * _mapScale + _mapPan[1];

            await context.LineWidthAsync(3.0 * Math.Sqrt(_mapScale));
            await context.StrokeStyleAsync(GetMapObjectColor(airField));
            await context.BeginPathAsync();
            await context.MoveToAsync(sx, sy);
            await context.LineToAsync(ex, ey);
            await context.StrokeAsync();
        }

        private async Task DrawMapObjectAsync(Context2D context, MapObject mapObject)
        {
            var x = _mapOptions.Width * mapObject.x ?? 0;
            var y = _mapOptions.Height * mapObject.y ?? 0;

            await context.FillStyleAsync(GetMapObjectColor(mapObject));
            await context.LineWidthAsync(1.0);
            await context.StrokeStyleAsync("#000");

            await context.FontAsync("bold 18pt Icons");
            await context.TextBaseLineAsync(TextBaseLine.Middle);
            await context.TextAlignAsync(TextAlign.Center);

            var iconCode = GetMapIconCode(mapObject.Icon ?? ".");

            var sx = x * _mapScale + _mapPan[0];
            var sy = y * _mapScale + _mapPan[1];

            var rotate = mapObject.Type == "respawn_base_fighter" || mapObject.Type == "respawn_base_bomber";

            if (rotate)
            {
                await context.SaveAsync();
                await context.TranslateAsync(sx, sy);
                var heading = Math.Atan2(mapObject.dx ?? 0, -(mapObject.dy ?? 0));
                await context.RotateAsync(heading);
                await context.TranslateAsync(-sx, -sy);
                await context.FillTextAsync(iconCode, sx, sy);
                await context.StrokeTextAsync(iconCode, sx, sy);
                await context.RestoreAsync();
            }
            else
            {
                await context.FillTextAsync(iconCode, sx, sy);
                await context.StrokeTextAsync(iconCode, sx, sy);
            }

        }

        private string GetMapIconCode(string icon)
        {
            return icon switch
            {
                "Airdefense" => "4",
                "Structure" => "5",
                "waypoint" => "6",
                "capture_zone" => "7",
                "bombing_point" => "8",
                "defending_point" => "9",
                "respawn_base_tank" => "0",
                "respawn_base_fighter" => ".",
                "respawn_base_bomber" => ":",
                _ => icon[..1],
            };
        }

        private async Task DrawPlayerAsync(Context2D context, MapObject player)
        {
            var x = player.x ?? 0;
            var y = player.y ?? 0;

            var direction = new Vector2(player.dx ?? 0, player.dy ?? 0);

            if (_lastPlayerPosition != null)
            {
                var x0 = _lastPlayerPosition.Value.X;
                var y0 = _lastPlayerPosition.Value.Y;

                if (Math.Abs(x0 - x) < 0.01)
                {
                    //x = MathHelpers.Approach(x0, x, dt, 0.4);
                }
            }

            await context.FillStyleAsync("#fff");
            await context.LineWidthAsync(2);
            await context.StrokeStyleAsync("#333");
            await context.BeginPathAsync();
            var w = 7.0;
            var l = 25.0;
            var dx = direction.X;
            var dy = direction.Y;
            var sx = x * _mapOptions.Width * _mapScale + _mapPan[0];
            var sy = y * _mapOptions.Height * _mapScale + _mapPan[1];

            sx -= l * 0.5 * dx;
            sy -= l * 0.5 * dy;

            await context.MoveToAsync(sx - w * dy, sy + w * dx);
            await context.LineToAsync(sx + w * dy, sy - w * dx);
            await context.LineToAsync(sx + l * dx, sy + l * dy);
            await context.ClosePathAsync();
            await context.FillAsync(FillRule.EvenOdd);
            await context.StrokeAsync();
        }

        private string GetMapObjectColor(MapObject mapObject)
        {
            // TODO: Blinking!
            if (mapObject.Blink != 0)
            {
                var bv = mapObject.Blink == 2 ? _blinkHeavyValue : _blinkNormalValue;

                var secondaryColor = new double[] { 255, 255, 0 };

                var blinkColor = new int[]
                {
                    Convert.ToInt32(Math.Floor(Lerp(mapObject.ColorArray[0], secondaryColor[0], bv))),
                    Convert.ToInt32(Math.Floor(Lerp(mapObject.ColorArray[1], secondaryColor[1], bv))),
                    Convert.ToInt32(Math.Floor(Lerp(mapObject.ColorArray[2], secondaryColor[2], bv))),
                };

                var colorR = blinkColor[0].ToString("X2");
                var colorG = blinkColor[1].ToString("X2");
                var colorB = blinkColor[2].ToString("X2");

                return $"#{colorR}{colorG}{colorB}";

            }
            else
            {
                return mapObject.ColorHex != null ? mapObject.ColorHex : "#FFFFFF";
            }
        }

        private double Lerp(double a, double b, double k)
        {
            return a * (1.0 - k) + b * k;
        }

        private async Task DrawMapGridAsync()
        {
            if (_canvas != null && _mapInformation != null)
            {
                await using var context = await _canvas.GetContext2DAsync();

                var scX = _mapOptions.Width * _mapScale / (_mapInformation.MapMax[0] - _mapInformation.MapMin[0]);
                var scY = _mapOptions.Width * _mapScale / (_mapInformation.MapMax[1] - _mapInformation.MapMin[1]);

                var firstVisCellX = Math.Floor((-_mapPan[0] / scX) / _mapInformation.GridSteps[0]);
                var firstVisCellY = Math.Floor((-_mapPan[1] / scY) / _mapInformation.GridSteps[1]);
                var xVis0 = _mapInformation.MapMin[0] + firstVisCellX * _mapInformation.GridSteps[0];
                var yVis0 = _mapInformation.MapMin[1] + firstVisCellY * _mapInformation.GridSteps[1];
                var xVis1 = _mapInformation.MapMin[0] + (Math.Ceiling((_mapOptions.Width - _mapPan[0]) / scX / _mapInformation.GridSteps[0]) * _mapInformation.GridSteps[0]);
                var yVis1 = _mapInformation.MapMin[1] + (Math.Ceiling((_mapOptions.Height - _mapPan[1]) / scY / _mapInformation.GridSteps[1]) * _mapInformation.GridSteps[1]);

                await context.LineWidthAsync(1);
                await context.StrokeStyleAsync("#555");

                await context.BeginPathAsync();

                for (var y = yVis0; y <= yVis1; y += _mapInformation.GridSteps[1])
                {
                    var yy = Math.Floor((y - _mapInformation.MapMin[1]) * scY + _mapPan[1]) + 0.5;
                    await context.MoveToAsync(0, yy);
                    await context.LineToAsync(_mapOptions.Width, yy);
                }

                for (var x = xVis0; x <= xVis1; x += _mapInformation.GridSteps[0])
                {
                    var xx = Math.Floor((x - _mapInformation.MapMin[0]) * scX + _mapPan[0]) + 0.5;
                    await context.MoveToAsync(xx, 0);
                    await context.LineToAsync(xx, _mapOptions.Height);
                }

                await context.StrokeAsync();

                // Make the grid labels have a outline so they are readable on both dark and light maps.
                await context.ShadowColorAsync("white");
                await context.ShadowOffsetXAsync(0);
                await context.ShadowOffsetYAsync(0);
                await context.ShadowBlurAsync(10);

                // Draw the up/down grid labels
                await context.FillStyleAsync("#111");
                await context.FontAsync("bold 9pt sans-serif");


                await context.TextBaseLineAsync(TextBaseLine.Middle);
                await context.TextAlignAsync(TextAlign.Left);

                for (double y = yVis0 + _mapInformation.GridSteps[1] * 0.5, n = firstVisCellY; y <= yVis1; y += _mapInformation.GridSteps[1], n++)
                {
                    var yy = Math.Floor((y - _mapInformation.MapMin[1]) * scY + _mapPan[1]) + 0.5;

                    int gridNumber = Convert.ToInt32(Math.Floor(n));
                    var gridLabel = Convert.ToChar(65 + gridNumber).ToString();

                    await context.FillTextAsync(gridLabel, 3, yy);
                }

                // Draw the left/right grid labels
                await context.TextBaseLineAsync(TextBaseLine.Top);
                await context.TextAlignAsync(TextAlign.Center);

                for (double x = xVis0 + _mapInformation.GridSteps[0] * 0.5, n = firstVisCellX; x <= xVis1; x += _mapInformation.GridSteps[0], n++)
                {
                    var xx = Math.Floor((x - _mapInformation.MapMin[0]) * scX + _mapPan[0]) + 0.5;
                    int gridNumber = Convert.ToInt32(Math.Floor(n));
                    var gridLabel = (1 + gridNumber).ToString();
                    await context.FillTextAsync(gridLabel, xx, 3);
                }

                // Remove the blur effecvt
                await context.ShadowBlurAsync(0);

            }
        }

        private (double? x, double? y) NormalizeLocation(double? x, double? y)
        {
            if (_mapInformation != null && x != null && y != null)
            {
                return (x, y);
            }
            else
            {
                return (null, null);
            }
        }
    }
}
