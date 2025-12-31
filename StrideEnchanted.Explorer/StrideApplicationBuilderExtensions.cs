using System;
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
    builder.Services.Configure<StrideExplorerOptions>(builder.Configuration.GetSection(StrideExplorerOptions.OptionsName));
    if (configureOptions != null)
      builder.Services.PostConfigure(configureOptions);

    builder.Services.AddHostedService(provider =>
    {
      var webHostBuilder = new WebHostBuilder();

      var environment = provider.GetRequiredService<IHostEnvironment>();
      var configuration = provider.GetRequiredService<IConfiguration>();
      var loggerProviders = provider.GetServices<ILoggerProvider>();

      webHostBuilder
        .UseContentRoot(environment.ContentRootPath)
        .ConfigureAppConfiguration(configBuilder =>
        {
          configBuilder.AddConfiguration(configuration);
        });

      webHostBuilder.ConfigureLogging(loggingBuilder =>
      {
        loggingBuilder
          .ClearProviders()
          .AddFilter("Microsoft.AspNetCore", LogLevel.Warning)
          .AddConfiguration(configuration.GetSection("Logging"));

        foreach (var loggerProvider in loggerProviders)
          loggingBuilder.AddProvider(loggerProvider);
      });

      webHostBuilder.ConfigureServices(services =>
      {
        services.Configure<StrideExplorerOptions>(configuration.GetSection(StrideExplorerOptions.OptionsName));
        if (configureOptions != null)
          services.PostConfigure(configureOptions);

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
      });

      webHostBuilder.UseKestrel(static kestrelOptions =>
      {
        var options = kestrelOptions.ApplicationServices
          .GetRequiredService<IOptions<StrideExplorerOptions>>().Value;

        Action<ListenOptions> listenOptions = static _ => { };
        if (options.UseHttps)
          listenOptions = static opt => opt.UseHttps();

        kestrelOptions.Listen(IPAddress.Parse(options.IPAddress), options.Port, listenOptions);
      });

      webHostBuilder.Configure(static app =>
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
      });

      webHostBuilder.UseStaticWebAssets();

      var webHost = webHostBuilder.Build();
      var logger = webHost.Services.GetRequiredService<ILogger<IStrideApplicationBuilder>>();
      if (environment.IsProduction())
        logger.LogWarning("Stride Explorer should not be used in production!");

      return new WebHostAdapter(webHost);
    });

    return builder;
  }
}
