using MassiveUDFMaker;
using MassiveUDFMaker.Code;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

using IHost host = Host.CreateApplicationBuilder(args).Build();
var config = host.Services.GetRequiredService<IConfiguration>();
var applicationSettings = config.Get<ApplicationSettings>();
Domain.CreateDomain(applicationSettings).Run();

await host.RunAsync();