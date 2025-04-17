using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stride.Games;

namespace StrideEnchanted.Host;

internal sealed class StrideApplicationBuilder<TGame> : IStrideApplicationBuilder
  where TGame : GameBase
{
  #region Fields and Properties

  private readonly HostApplicationBuilder hostApplicationBuilder;
  private bool isHostBuilded = false;

  #endregion

  #region Constructor

  internal StrideApplicationBuilder(HostApplicationBuilderSettings settings)
  {
    this.hostApplicationBuilder = Microsoft.Extensions.Hosting.Host.CreateEmptyApplicationBuilder(settings);
    this.Initialize();
  }

  #endregion

  #region Methods

  private void Initialize()
  {
#warning TODO: нужно перенести сервисы из Game.Services.
    var serviceCollection = this.hostApplicationBuilder.Services
      .AddSingleton(this.hostApplicationBuilder.Services)
      .AddSingleton<TGame>()
      .AddSingleton<IGame>(static p => p.GetRequiredService<TGame>())
      .AddSingleton<GameBase>(static p => p.GetRequiredService<TGame>());
  }

  #endregion

  #region IStrideApplicationBuilder

  public IDictionary<object, object> Properties => ((IHostApplicationBuilder)this.hostApplicationBuilder).Properties;

  public IConfigurationManager Configuration => this.hostApplicationBuilder.Configuration;

  public IHostEnvironment Environment => this.hostApplicationBuilder.Environment;

  public ILoggingBuilder Logging => this.hostApplicationBuilder.Logging;

  public IMetricsBuilder Metrics => this.hostApplicationBuilder.Metrics;

  public IServiceCollection Services => this.hostApplicationBuilder.Services;

  public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null)
    where TContainerBuilder : notnull
  {
    this.hostApplicationBuilder.ConfigureContainer(factory, configure);
  }

  public StrideApplication Build()
  {
    if (this.isHostBuilded)
      throw new InvalidOperationException("Host already builded.");
    this.isHostBuilded = true;

    var host = this.hostApplicationBuilder.Build();
    var game = host.Services.GetRequiredService<GameBase>();
    return new StrideApplication(host, game);
  }

  #endregion
}
