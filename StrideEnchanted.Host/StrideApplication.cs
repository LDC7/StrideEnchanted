using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stride.Games;

namespace StrideEnchanted.Host;

public sealed class StrideApplication : IHost, IAsyncDisposable
{
  #region Fields and Properties

  private readonly IHost host;

  #endregion

  #region Constructor

  internal StrideApplication(IHost host)
  {
    this.host = host;
  }

  #endregion

  #region Methods

  public static IStrideApplicationBuilder CreateBuilder<TGame>(string[] args)
    where TGame : class, IGame
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
    await ((IHost)this).StartAsync(cancellationToken).ConfigureAwait(false);
    await this.WaitForShutdownAsync(cancellationToken).ConfigureAwait(false);
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
