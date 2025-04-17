using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stride.Games;

namespace StrideEnchanted.Host;

public sealed class StrideApplication : IHost, IAsyncDisposable
{
  #region Fields and Properties

  private readonly object locker = new();
  private readonly IHost host;
  private readonly GameBase game;
  private readonly GameContext? gameContext;

  private Task? gameRunningTask;
  private Task? hostStopingTask;

  #endregion

  #region Constructor

  internal StrideApplication(IHost host, GameBase game, GameContext? gameContext)
  {
    this.host = host;
    this.game = game;
    this.gameContext = gameContext;
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

  public async Task RunAsync(CancellationToken cancellationToken = default)
  {
    this.gameRunningTask = this.RunGameAsync(cancellationToken);
    await ((IHost)this).StartAsync(cancellationToken).ConfigureAwait(false);
    await Task.WhenAll(this.WaitForShutdownAsync(cancellationToken), this.gameRunningTask).ConfigureAwait(false);
  }

  private Task RunGameAsync(CancellationToken cancellationToken)
  {
    this.game.WindowCreated += (windowCreatedEventOwner, windowCreatedEvent) =>
    {
      this.game.Window.Closing += (windowClosingEventOwner, windowClosingEvent) =>
      {
        _ = this.StopInternal(CancellationToken.None);
      };

      this.game.Exiting += (gameExitingEventOwner, gameExitingEvent) =>
      {
        _ = this.StopInternal(CancellationToken.None);
      };
    };

    return Task.Run(() => this.game.Run(this.gameContext), cancellationToken);
  }

  private Task StopInternal(CancellationToken cancellationToken)
  {
    if (this.hostStopingTask == null)
    {
      lock (this.locker)
      {
        if (this.hostStopingTask == null)
          this.hostStopingTask = ((IHost)this).StopAsync(cancellationToken);
      }
    }

    return this.hostStopingTask;
  }

  #endregion

  #region IHost

  public IServiceProvider Services => this.host.Services;

  Task IHost.StartAsync(CancellationToken cancellationToken)
  {
    return this.host.StartAsync(cancellationToken);
  }

  Task IHost.StopAsync(CancellationToken cancellationToken)
  {
    return this.host.StopAsync(cancellationToken);
  }

  void IDisposable.Dispose()
  {
    this.game.Dispose();
    this.host.Dispose();
  }

  #endregion

  #region IAsyncDisposable

  public ValueTask DisposeAsync()
  {
    this.game.Dispose();
    return ((IAsyncDisposable)this.host).DisposeAsync();
  }

  #endregion
}
