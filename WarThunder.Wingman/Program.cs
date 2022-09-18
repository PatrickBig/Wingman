using WarThunder.Wingman;
using Blazored.LocalStorage;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure endpoint default
builder.Services.Configure<WingmanOptions>(builder.Configuration.GetSection(nameof(WingmanOptions)));
var warThunderOptions = builder.Configuration.GetSection(nameof(WingmanOptions)).Get<WingmanOptions>();

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(warThunderOptions.WarThunderEndpoint) });
builder.Services.AddBlazorise(options =>
{
    options.Immediate = true;
})
.AddBootstrapProviders()
.AddFontAwesomeIcons();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddWingmanServices();

await builder.Build().RunAsync();
