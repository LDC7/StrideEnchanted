using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stride.Engine;
using Stride.Games;

namespace StrideEnchanted.Host;

public sealed class StrideApplication : IDisposable, IAsyncDisposable
{
  #region Fields and Properties

  private readonly SemaphoreSlim stopSemaphore = new(1, 1);
  private readonly IHost host;
  private readonly GameBase game;
  private readonly ILogger<StrideApplication> logger;
  private bool disposed;

  private GameContext? gameContext;
  private Task? hostStoppingTask;

  public IServiceProvider Services => this.host.Services;

  #endregion

  #region Constructor

  internal StrideApplication(IHost host, GameBase game)
  {
    this.host = host;
    this.game = game;
    this.logger = this.host.Services.GetRequiredService<ILogger<StrideApplication>>();
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
    try
    {
      await this.RunGame(cancellationToken).ConfigureAwait(false);
    }
    finally
    {
      await this.StopHostAsync(CancellationToken.None).ConfigureAwait(false);
    }
  }

  private Task RunGame(CancellationToken cancellationToken)
  {
    var gameContext = this.gameContext ?? this.host.Services.GetService<GameContext>();

    // CancellationToken does not propagate into Stride's game loop; it only gates task creation.
    return Task.Run(() => this.game.Run(gameContext), cancellationToken);
  }

  private async Task StopHostAsync(CancellationToken cancellationToken)
  {
    var existingTask = this.hostStoppingTask;
    if (existingTask != null)
    {
      await existingTask.ConfigureAwait(false);
      return;
    }

    await this.stopSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      this.hostStoppingTask ??= this.host.StopAsync(cancellationToken);
    }
    finally
    {
      this.stopSemaphore.Release();
    }

    await this.hostStoppingTask.ConfigureAwait(false);
  }

  #endregion

  #region IDisposable

  void IDisposable.Dispose()
  {
    this.DisposeAsync().AsTask().GetAwaiter().GetResult();
  }

  #endregion

  #region IAsyncDisposable

  public async ValueTask DisposeAsync()
  {
    if (this.disposed)
      return;

    this.disposed = true;

    await this.StopHostAsync(CancellationToken.None).ConfigureAwait(false);

    var disposeTask = Task.Run(this.game.Dispose);
    var completedTask = await Task.WhenAny(disposeTask, Task.Delay(TimeSpan.FromSeconds(5)));
    
    if (completedTask == disposeTask)
    {
      await disposeTask.ConfigureAwait(false);
    }
    else
    {
      this.logger.LogWarning("Game dispose is taking longer than 5 seconds; continuing shutdown.");
    }

    await ((IAsyncDisposable)this.host).DisposeAsync().ConfigureAwait(false);
  }

  #endregion
}
