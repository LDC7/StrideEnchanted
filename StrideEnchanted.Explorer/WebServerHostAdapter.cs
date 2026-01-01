using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StrideEnchanted.Explorer;

internal sealed class WebServerHostAdapter : IHostedService, IDisposable
{
  #region Fields and Properties

  private readonly IHost host;
  private readonly ILogger<WebServerHostAdapter> logger;
  private bool disposed;

  #endregion

  #region Constructor

  public WebServerHostAdapter(IHost host)
  {
    this.host = host ?? throw new ArgumentNullException(nameof(host));
    this.logger = this.host.Services.GetRequiredService<ILogger<WebServerHostAdapter>>();
  }

  #endregion

  #region IHostedService

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    ObjectDisposedException.ThrowIf(this.disposed, this);

    await this.host.StartAsync(cancellationToken);

    var server = this.host.Services.GetService<IServer>();
    var addresses = server?.Features.Get<IServerAddressesFeature>();
    if (addresses?.Addresses != null)
    {
      foreach (var address in addresses.Addresses)
        this.logger.LogInformation("Stride Explorer is listening on {Address}", address);
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    if (this.disposed)
      return;

    try
    {
      await this.host.StopAsync(cancellationToken);
    }
    finally
    {
      this.Dispose();
    }
  }

  #endregion

  #region IDisposable

  public void Dispose()
  {
    if (this.disposed)
      return;

    this.disposed = true;
    this.host.Dispose();
  }

  #endregion
}
