using WarThunder.Wingman.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WingmanServices
    {
        public static IServiceCollection AddWingmanServices(this IServiceCollection services)
        {
            services.AddScoped<IClientSettings, ClientSettings>();
            services.AddSingleton<IGameInformationService, GameInformationService>();
            services.AddTransient<MapInformationService>();
            return services;
        }
    }
}
