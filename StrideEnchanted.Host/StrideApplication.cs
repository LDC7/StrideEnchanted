using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stride.Games;

namespace StrideEnchanted.Host;

public sealed class StrideApplication : IDisposable, IAsyncDisposable
{
  #region Fields and Properties

  private readonly object locker = new();
  private readonly IHost host;
  private readonly GameBase game;

  private GameContext? gameContext;
  private Task? hostStopingTask;

  public IServiceProvider Services => this.host.Services;

  #endregion

  #region Constructor

  internal StrideApplication(IHost host, GameBase game)
  {
    this.host = host;
    this.game = game;
  }

  #endregion

  #region Methods

  public static IStrideApplicationBuilder CreateBuilder<TGame>(string[] args)
    where TGame : GameBase
  {
    var settings = new HostApplicationBuilderSettings()
    {
      Args = args,
      DisableDefaults = true
    };

    return new StrideApplicationBuilder<TGame>(settings);
  }

  public StrideApplication UseGameContext(GameContext? gameContext)
  {
    this.gameContext = gameContext;
    return this;
  }

  public async Task RunAsync(CancellationToken cancellationToken = default)
  {
    var gameRunningTask = this.RunGame(cancellationToken);
    var hostRunningTask = this.host.StartAsync(cancellationToken);
    await gameRunningTask.ConfigureAwait(false);
    await hostRunningTask.ConfigureAwait(false);
    _ = this.StopHost(CancellationToken.None).ConfigureAwait(false);
  }

  private Task RunGame(CancellationToken cancellationToken)
  {
    this.game.WindowCreated += (windowCreatedEventOwner, windowCreatedEvent) =>
    {
      this.game.Window.Closing += (windowClosingEventOwner, windowClosingEvent) =>
      {
        _ = this.StopHost(CancellationToken.None);
      };

      this.game.Exiting += (gameExitingEventOwner, gameExitingEvent) =>
      {
        _ = this.StopHost(CancellationToken.None);
      };
    };

    var gameContext = this.gameContext ?? this.host.Services.GetService<GameContext>();
    return Task.Run(() => this.game.Run(gameContext), cancellationToken);
  }

  private Task StopHost(CancellationToken cancellationToken)
  {
    if (this.hostStopingTask == null)
    {
      lock (this.locker)
      {
        if (this.hostStopingTask == null)
          this.hostStopingTask = this.host.StopAsync(cancellationToken);
      }
    }

    return this.hostStopingTask;
  }

  #endregion

  #region IDisposable

  void IDisposable.Dispose()
  {
    this.host.Dispose();
  }

  #endregion

  #region IAsyncDisposable

  public ValueTask DisposeAsync()
  {
    return ((IAsyncDisposable)this.host).DisposeAsync();
  }

  #endregion
}
