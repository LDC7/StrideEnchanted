using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using StrideEnchanted.Explorer.Components;
using StrideEnchanted.Explorer.Interfaces;
using StrideEnchanted.Explorer.Options;
using StrideEnchanted.Host;

namespace StrideEnchanted.Explorer.Services;

internal sealed class KestrelExplorerHostFactory : IExplorerHostFactory
{
  #region Fields and Properties

  private readonly IExplorerServicesConfigurator servicesConfigurator;

  #endregion

  #region Constructor

  public KestrelExplorerHostFactory(IExplorerServicesConfigurator servicesConfigurator)
  {
    this.servicesConfigurator = servicesConfigurator;
  }

  #endregion

  #region IExplorerHostFactory

  public IExplorerHost CreateHost(IServiceProvider provider, Action<StrideExplorerOptions>? configureOptions)
  {
    var host = this.BuildExplorerHost(provider, configureOptions);
    return new WebServerHostAdapter(host);
  }

  #endregion

  #region Methods

  private IHost BuildExplorerHost(
    IServiceProvider provider,
    Action<StrideExplorerOptions>? configureOptions)
  {
    var environment = provider.GetRequiredService<IHostEnvironment>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    var loggerProviders = provider.GetServices<ILoggerProvider>();

    var webApplicationBuilder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
      ApplicationName = typeof(StrideApplicationBuilderExtensions).Assembly.GetName().Name!,
      EnvironmentName = environment.EnvironmentName,
      ContentRootPath = environment.ContentRootPath
    });

    webApplicationBuilder.Configuration.Sources.Clear();
    webApplicationBuilder.Configuration.AddConfiguration(configuration);
    StaticWebAssetsLoader.UseStaticWebAssets(webApplicationBuilder.Environment, webApplicationBuilder.Configuration);

    webApplicationBuilder.WebHost
      .UseStaticWebAssets()
      .UseKestrel(this.ConfigureKestrel);

    this.ConfigureLogging(webApplicationBuilder.Logging, configuration, loggerProviders);

    this.servicesConfigurator.ConfigureServices(webApplicationBuilder.Services, provider, configuration, configureOptions);

    webApplicationBuilder.Services
      .AddRouting()
      .AddRazorPages();

    webApplicationBuilder.Services
      .AddMudServices()
      .AddRazorComponents()
      .AddInteractiveServerComponents();

    var webApplication = webApplicationBuilder.Build();
    this.ConfigurePipeline(webApplication);

    var logger = webApplication.Services.GetRequiredService<ILogger<IStrideApplicationBuilder>>();
    this.WarnIfProduction(environment, logger);

    return webApplication;
  }

  private void ConfigureLogging(
    ILoggingBuilder loggingBuilder,
    IConfiguration configuration,
    IEnumerable<ILoggerProvider> loggerProviders)
  {
    loggingBuilder
      .ClearProviders()
      .AddFilter("Microsoft.AspNetCore", LogLevel.Warning)
      .AddConfiguration(configuration.GetSection("Logging"));

    foreach (var loggerProvider in loggerProviders)
      loggingBuilder.AddProvider(loggerProvider);
  }

  private void ConfigureKestrel(KestrelServerOptions kestrelOptions)
  {
    var explorerOptions = kestrelOptions.ApplicationServices
      .GetRequiredService<IOptions<StrideExplorerOptions>>().Value;

    var address = IPAddress.TryParse(explorerOptions.IPAddress, out var parsed)
      ? parsed
      : IPAddress.Loopback;

    kestrelOptions.Listen(address, explorerOptions.Port, listenOptions =>
    {
      if (explorerOptions.UseHttps)
        listenOptions.UseHttps();
    });
  }

  private void ConfigurePipeline(IApplicationBuilder app)
  {
    app.UseExceptionHandler("/Error");
    app.UseRouting();
    app.UseAntiforgery();

    app.UseEndpoints(endpoints =>
    {
      endpoints.MapStaticAssets();
      endpoints
        .MapRazorComponents<AppView>()
        .AddInteractiveServerRenderMode()
        .WithStaticAssets();
    });
  }

  private void WarnIfProduction(IHostEnvironment environment, ILogger logger)
  {
    if (environment.IsProduction())
      logger.LogWarning("Stride Explorer is intended for diagnostics and should not run in production.");
  }

  #endregion
}
