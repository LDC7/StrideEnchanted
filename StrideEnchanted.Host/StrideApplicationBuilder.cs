using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stride.Audio;
using Stride.Core.IO;
using Stride.Engine;
using Stride.Engine.Processors;
using Stride.Games;
using Stride.Graphics;
using Stride.Graphics.Font;
using Stride.Profiling;
using Stride.Rendering.Sprites;
using Stride.VirtualReality;
using StrideEnchanted.Host.Extensions;
using StrideEnchanted.Host.Logging;

namespace StrideEnchanted.Host;

internal sealed class StrideApplicationBuilder<TGame> : IStrideApplicationBuilder
  where TGame : GameBase
{
  #region Fields and Properties

  private readonly HostApplicationBuilder hostApplicationBuilder;
  private int buildWasCalled;

  #endregion

  #region Constructor

  internal StrideApplicationBuilder(HostApplicationBuilderSettings settings)
  {
    ArgumentNullException.ThrowIfNull(settings);

    this.hostApplicationBuilder = Microsoft.Extensions.Hosting.Host.CreateEmptyApplicationBuilder(settings);
    this.Initialize();
  }

  #endregion

  #region Methods

  private void Initialize()
  {
    this.ConfigureConfiguration();
    this.ConfigureLogging();
    this.RegisterGameServices();
  }

  private void ConfigureConfiguration()
  {
    this.hostApplicationBuilder.Configuration
      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
      .AddJsonFile($"appsettings.{this.hostApplicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
      .AddEnvironmentVariables();
  }

  private void ConfigureLogging()
  {
    this.hostApplicationBuilder.Logging
      .ClearProviders()
      .AddProvider(new StrideLoggerProvider());
  }

  private void RegisterGameServices()
  {
    var services = this.hostApplicationBuilder.Services;

    _ = services
      .AddSingleton(this.hostApplicationBuilder.Services)
      .AddSingleton<TGame>()
      .AddSingleton<IGame>(static p => p.GetRequiredService<TGame>())
      .AddSingleton<GameBase>(static p => p.GetRequiredService<TGame>());

    _ = services
      // Game
      .AddGameService<ScriptSystem>()
      .AddGameService<SceneSystem>()
      .AddGameService<AudioSystem>()
      .AddGameService<IAudioEngineProvider>()
      .AddGameService<FontSystem>()
      .AddGameService<IFontFactory>()
      .AddGameService<SpriteAnimationSystem>()
      .AddGameService<DebugTextSystem>()
      .AddGameService<GameProfilingSystem>()
      .AddGameService<VRDeviceSystem>()
      .AddGameService<IGraphicsDeviceManager>()
      .AddGameService<IGraphicsDeviceService>()
      // GameBase
      .AddGameService<IDatabaseFileProviderService>()
      .AddGameService<IGameSystemCollection>()
      .AddGameService<IGraphicsDeviceFactory>()
      .AddGameService<IGamePlatform>();
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
    if (Interlocked.Exchange(ref this.buildWasCalled, 1) != 0)
      throw new InvalidOperationException("Host has already been built.");

    var host = this.hostApplicationBuilder.Build();
    var game = host.Services.GetRequiredService<GameBase>();

    game.Services.AddService(host.Services);

    return new StrideApplication(host, game);
  }

  #endregion
}
