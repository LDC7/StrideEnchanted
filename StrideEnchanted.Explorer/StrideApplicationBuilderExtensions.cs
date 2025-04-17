using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Stride.Games;
using StrideEnchanted.Explorer.Components;
using StrideEnchanted.Host;

namespace StrideEnchanted.Explorer;

public static class StrideApplicationBuilderExtensions
{
  public static IStrideApplicationBuilder AddStrideExplorer(this IStrideApplicationBuilder builder)
  {
    builder.Services.AddHostedService(static provider =>
    {
      var webHostBuilder = new WebHostBuilder();

      var environment = provider.GetRequiredService<IHostEnvironment>();
      var configuration = provider.GetRequiredService<IConfiguration>();

      webHostBuilder
        .UseContentRoot(environment.ContentRootPath)
        .ConfigureAppConfiguration(configBuilder =>
        {
          configBuilder.AddConfiguration(configuration);
        });

#warning TODO: Нужно использовать логирование, которое используется в хосте.
      webHostBuilder.ConfigureLogging(static loggingBuilder =>
      {
        loggingBuilder.AddConsole();
      });

      webHostBuilder.ConfigureServices(services =>
      {
        var game = provider.GetRequiredService<IGame>();
        services.AddSingleton(game);

        services
          .AddRouting()
          .AddRazorPages();

        services
          .AddMudServices()
          .AddRazorComponents()
          .AddInteractiveServerComponents();
      });

      webHostBuilder.UseKestrel(static options =>
      {
#warning TODO: Это должно быть как-то иначе.
        options.ListenLocalhost(51891);
      });

      webHostBuilder.Configure(static app =>
      {
        app.UseExceptionHandler("/Error");
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAntiforgery();

        app.UseEndpoints(endpoints =>
        {
          endpoints
            .MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        });
      });

      webHostBuilder.UseStaticWebAssets();

      var webHost = webHostBuilder.Build();
      var logger = webHost.Services.GetRequiredService<ILogger<IWebHost>>();
      if (environment.IsProduction())
        logger.LogWarning("Stride Explorer should not be used in production!");

      return new WebHostAdapter(webHost, logger);
    });

    return builder;
  }
}
