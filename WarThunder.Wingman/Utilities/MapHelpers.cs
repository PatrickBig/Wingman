namespace WarThunder.Wingman.Utilities
{
    public static class MapHelpers
    {
        public static string GetMapImageUrl(int mapGeneration) => $"map.img?gen={mapGeneration}";
    }
}
