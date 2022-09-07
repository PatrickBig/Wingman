using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WarThunder.Shared.Models
{
    public class MapObject
    {
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("color")]
        public string? ColorHex { get; set; }

        [JsonPropertyName("color[]")]
        public int[] ColorArray { get; set; } = Array.Empty<int>();

        public int Blink { get; set; }

        public string? Icon { get; set; }

        [JsonPropertyName("icon_bg")]
        public string? IconBackground { get; set; }

        public double? sx { get; set; }
        public double? sy { get; set; }
        public double? ex { get; set; }
        public double? ey { get; set; }
        public double? x { get; set; }
        public double? y { get; set; }
        public float? dx { get; set; }
        public float? dy { get; set; }
    }

}
