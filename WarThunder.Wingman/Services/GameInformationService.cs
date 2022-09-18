using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WarThunder.Wingman.Services
{
    public class GameInformationService : IGameInformationService, IDisposable
    {
        private readonly Timer _timer;
        private readonly HttpClient _http;
        private readonly ILogger<GameInformationService> _logger;

        private GameState _gameState = GameState.Unavailable;

        private Mission? _mission;

        private MapInformation? _mapInformation;

        public event EventHandler<GameState>? GameStateChanged;
        public event EventHandler<Mission>? MissionChanged;
        public event EventHandler<MapInformation>? MapInformationChanged;
        public event EventHandler<MapObject[]>? MapObjectsUpdated;

        public GameInformationService(HttpClient http, ILogger<GameInformationService> logger)
        {
            _timer = new Timer(async (stateInfo) =>
            {
                await UpdateGameStateAsync();
            }, null, 0, 500);
            _http = http;
            _logger = logger;
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _gameState = GameState.Unavailable;
                    if (_timer != null)
                    {
                        _timer.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public GameState GetGameState()
        {
            return _gameState;
        }

        private async Task UpdateGameStateAsync()
        {
            try
            {
                var missionResponse = await _http.GetAsync("mission.json");

                if (missionResponse != null)
                {
                    missionResponse.EnsureSuccessStatusCode();

                    var missionJson = await missionResponse.Content.ReadAsStringAsync();

                    if (missionJson.Length > 0)
                    {
                        UpdateGameState(GameState.InGame);

                        _mission = System.Text.Json.JsonSerializer.Deserialize<Mission>(missionJson);

                        // Fire other updates
                        UpdateMission(_mission);
                        await UpdateMapInformationAsync();
                        await UpdateMapObjectsAsync();
                    }
                    else
                    {
                        UpdateGameState(GameState.Unavailable);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message == "TypeError: Failed to fetch")
                {
                    UpdateGameState(GameState.Unavailable);
                }
                else
                {
                    throw;
                }
            }
        }

        private void UpdateGameState(GameState gameState)
        {
            if (gameState != _gameState)
            {
                _gameState = gameState;
                GameStateChanged?.Invoke(this, gameState);
            }
        }

        private void UpdateMission(Mission? mission)
        {
            if (mission != _mission)
            {
                if (mission != null)
                {
                    MissionChanged?.Invoke(this, mission);
                }

                _mission = mission;
            }
        }

        private async Task UpdateMapInformationAsync()
        {
            var mapInformation = await _http.GetFromJsonAsync<MapInformation>("map_info.json");

            if (mapInformation != null)
            {
                if (mapInformation.MapGeneration != _mapInformation?.MapGeneration)
                {
                    if (mapInformation != null)
                    {
                        MapInformationChanged?.Invoke(this, mapInformation);
                    }
                }

                _mapInformation = mapInformation;

            }
        }

        private async Task UpdateMapObjectsAsync()
        {
            if (GetGameState() == GameState.InGame)
            {
                var mapObjects = await _http.GetFromJsonAsync<MapObject[]>("map_obj.json");

                if (mapObjects != null)
                {
                    MapObjectsUpdated?.Invoke(this, mapObjects);
                }
            }
        }

        public Mission? GetMission()
        {
            return _mission;
        }

        public MapInformation? GetMapInformation()
        {
            return _mapInformation;
        }
    }
}
