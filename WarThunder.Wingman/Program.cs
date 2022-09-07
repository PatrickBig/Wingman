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
builder.Services.Configure<WarThunderOptions>(builder.Configuration.GetSection(nameof(WarThunderOptions)));
var warThunderOptions = builder.Configuration.GetSection(nameof(WarThunderOptions)).Get<WarThunderOptions>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(warThunderOptions.WarThunderEndpoint) });
builder.Services.AddBlazorise(options =>
{
    options.Immediate = true;
})
.AddBootstrapProviders()
.AddFontAwesomeIcons();

builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
