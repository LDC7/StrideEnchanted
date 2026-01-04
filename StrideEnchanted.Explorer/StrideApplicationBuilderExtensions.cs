using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StrideEnchanted.Explorer.Interfaces;
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

    builder.Services.TryAddSingleton<IExplorerServicesConfigurator, DefaultExplorerServicesConfigurator>();
    builder.Services.TryAddSingleton<IExplorerHostFactory, KestrelExplorerHostFactory>();

    builder.Services.AddHostedService(provider =>
      provider.GetRequiredService<IExplorerHostFactory>()
        .CreateHost(provider, configureOptions));

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
}
