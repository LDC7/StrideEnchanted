using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stride.Games;
using StrideEnchanted.Explorer.Interfaces;
using StrideEnchanted.Explorer.Options;

namespace StrideEnchanted.Explorer.Services;

internal sealed class DefaultExplorerServicesConfigurator : IExplorerServicesConfigurator
{
  public void ConfigureServices(
    IServiceCollection services,
    IServiceProvider hostServices,
    IConfiguration configuration,
    Action<StrideExplorerOptions>? configureOptions)
  {
    services.Configure<StrideExplorerOptions>(configuration.GetSection(StrideExplorerOptions.OptionsName));
    if (configureOptions != null)
      services.PostConfigure(configureOptions);

    services.TryAddSingleton(TimeProvider.System);

    var game = hostServices.GetRequiredService<IGame>();
    services.AddSingleton(game);
    services.TryAddSingleton<ISceneInstanceProvider, GameSceneInstanceProvider>();
    services.AddSingleton<DataTrackingTimer>();
  }
}

