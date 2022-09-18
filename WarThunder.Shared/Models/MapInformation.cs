using System.Text.Json.Serialization;

namespace WarThunder.Shared.Models
{

    public class MapInformation : IEquatable<MapInformation?>
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

        public override bool Equals(object? obj)
        {
            return Equals(obj as MapInformation);
        }

        public bool Equals(MapInformation? other)
        {
            return other is not null &&
                   EqualityComparer<float[]>.Default.Equals(GridSteps, other.GridSteps) &&
                   EqualityComparer<float[]>.Default.Equals(GridZero, other.GridZero) &&
                   MapGeneration == other.MapGeneration &&
                   EqualityComparer<float[]>.Default.Equals(MapMax, other.MapMax) &&
                   EqualityComparer<float[]>.Default.Equals(MapMin, other.MapMin);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GridSteps, GridZero, MapGeneration, MapMax, MapMin);
        }

        public static bool operator ==(MapInformation? left, MapInformation? right)
        {
            return EqualityComparer<MapInformation>.Default.Equals(left, right);
        }

        public static bool operator !=(MapInformation? left, MapInformation? right)
        {
            return !(left == right);
        }
    }

}
