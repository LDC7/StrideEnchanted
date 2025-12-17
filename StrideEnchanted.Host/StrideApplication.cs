using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stride.Engine;
using Stride.Games;

namespace StrideEnchanted.Host;

public sealed class StrideApplication : IDisposable, IAsyncDisposable
{
  #region Fields and Properties

  private readonly Lock locker = new();
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
    where TGame : Game
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
    await this.host.StartAsync(cancellationToken).ConfigureAwait(false);
    await this.RunGame(cancellationToken).ConfigureAwait(false);

    _ = this.StopHost(CancellationToken.None).ConfigureAwait(false);
  }

  private Task RunGame(CancellationToken cancellationToken)
  {
    var gameContext = this.gameContext ?? this.host.Services.GetService<GameContext>();

    // СancellationToken does not work here(
    return Task.Run(() => this.game.Run(gameContext), cancellationToken);
  }

  private Task StopHost(CancellationToken cancellationToken)
  {
    if (this.hostStopingTask == null)
    {
      lock (this.locker)
      {
        this.hostStopingTask ??= this.host.StopAsync(cancellationToken);
      }
    }

    return this.hostStopingTask;
  }

  #endregion

  #region IDisposable

  void IDisposable.Dispose()
  {
    this.DisposeAsync().AsTask().GetAwaiter().GetResult();
  }

  #endregion

  #region IAsyncDisposable

  public ValueTask DisposeAsync()
  {
    return ((IAsyncDisposable)this.host).DisposeAsync();
  }

  #endregion
}
