using System.Text.Json.Serialization;

namespace WarThunder.Shared.Models
{

    public class MapInformation
    {
        [JsonPropertyName("grid_steps")]
        public float[] GridSteps { get; set; } = Array.Empty<float>();

        [JsonPropertyName("grid_zero")]
        public float[] GridZero { get; set; } = Array.Empty<float>();

        [JsonPropertyName("map_generation")]
        public int MapGeneration { get; set; }

        [JsonPropertyName("map_max")]
        public float[] MapMax { get; set; } = Array.Empty<float>();

        [JsonPropertyName("map_min")]
        public float[] MapMin { get; set; } = Array.Empty<float>();
    }

}
