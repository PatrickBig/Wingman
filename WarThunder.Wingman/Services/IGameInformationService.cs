namespace WarThunder.Wingman.Services
{
    public interface IGameInformationService
    {
        public event EventHandler<GameState>? GameStateChanged;
        public event EventHandler<Mission>? MissionChanged;
        public event EventHandler<MapInformation>? MapInformationChanged;
        public event EventHandler<MapObject[]>? MapObjectsUpdated;

        public GameState GetGameState();

        public Mission? GetMission();

        public MapInformation? GetMapInformation();
    }
}
