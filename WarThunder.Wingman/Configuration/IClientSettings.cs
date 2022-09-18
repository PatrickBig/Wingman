namespace WarThunder.Wingman.Configuration
{
    public interface IClientSettings
    {
        public Task<WingmanOptions> GetOptionsAsync(CancellationToken? cancellationToken);
    }
}
