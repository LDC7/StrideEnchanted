using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using Stride.Games;
using StrideEnchanted.Explorer.Components;
using StrideEnchanted.Explorer.Options;
using StrideEnchanted.Explorer.Services;
using StrideEnchanted.Host;

namespace StrideEnchanted.Explorer;

public static class StrideApplicationBuilderExtensions
{
  public static IStrideApplicationBuilder AddStrideExplorer(
    this IStrideApplicationBuilder builder,
    Action<StrideExplorerOptions>? configureOptions = null)
  {
    ArgumentNullException.ThrowIfNull(builder);

    ConfigureOptions(builder.Services, builder.Configuration, configureOptions);

    builder.Services.AddHostedService(provider => BuildExplorerHost(provider, configureOptions));

    return builder;
  }

  private static void ConfigureOptions(
    IServiceCollection services,
    IConfiguration configuration,
    Action<StrideExplorerOptions>? configureOptions)
  {
    services.Configure<StrideExplorerOptions>(configuration.GetSection(StrideExplorerOptions.OptionsName));
    if (configureOptions != null)
      services.PostConfigure(configureOptions);
  }

  private static IHostedService BuildExplorerHost(
    IServiceProvider provider,
    Action<StrideExplorerOptions>? configureOptions)
  {
    var environment = provider.GetRequiredService<IHostEnvironment>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    var loggerProviders = provider.GetServices<ILoggerProvider>();

    var webHostBuilder = new WebHostBuilder()
      .UseContentRoot(environment.ContentRootPath)
      .UseStaticWebAssets()
      .ConfigureAppConfiguration(config => config.AddConfiguration(configuration))
      .ConfigureLogging(logging => ConfigureLogging(logging, configuration, loggerProviders))
      .ConfigureServices(services => ConfigureServices(services, provider, configuration, configureOptions))
      .UseKestrel(ConfigureKestrel)
      .Configure(ConfigurePipeline);

    var webHost = webHostBuilder.Build();
    WarnIfProduction(environment, webHost.Services.GetRequiredService<ILogger<IStrideApplicationBuilder>>());

    return new WebHostAdapter(webHost);
  }

  private static void ConfigureLogging(
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

  private static void ConfigureServices(
    IServiceCollection services,
    IServiceProvider provider,
    IConfiguration configuration,
    Action<StrideExplorerOptions>? configureOptions)
  {
    ConfigureOptions(services, configuration, configureOptions);

    var game = provider.GetRequiredService<IGame>();
    services
      .AddSingleton(game)
      .AddSingleton<DataTrackingTimer>();

    services
      .AddRouting()
      .AddRazorPages();

    services
      .AddMudServices()
      .AddRazorComponents()
      .AddInteractiveServerComponents();
  }

  private static void ConfigureKestrel(KestrelServerOptions kestrelOptions)
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

  private static void ConfigurePipeline(IApplicationBuilder app)
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

  private static void WarnIfProduction(IHostEnvironment environment, ILogger logger)
  {
    if (environment.IsProduction())
      logger.LogWarning("Stride Explorer is intended for diagnostics and should not run in production.");
  }
}
