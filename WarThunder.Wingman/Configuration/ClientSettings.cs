using Blazored.LocalStorage;

namespace WarThunder.Wingman.Configuration
{
    public class ClientSettings : IClientSettings
    {
        private readonly ILocalStorageService _localStorage;
        private const string OptionsKeyName = "WingmanOptions";

        public ClientSettings(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<WingmanOptions> GetOptionsAsync(CancellationToken? cancellationToken = null)
        {
            var options = await _localStorage.GetItemAsync<WingmanOptions>(OptionsKeyName, cancellationToken);

            if (options == null)
            {
                options = new();
                await _localStorage.SetItemAsync(OptionsKeyName, options, cancellationToken);
            }

            return options;
        }
    }
}
