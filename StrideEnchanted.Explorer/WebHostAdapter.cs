using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StrideEnchanted.Explorer;

internal sealed class WebHostAdapter : IHostedService, IDisposable
{
  #region Fields and Properties

  private readonly IWebHost webHost;
  private readonly ILogger<WebHostAdapter> logger;
  private bool disposed;

  #endregion

  #region Constructor

  public WebHostAdapter(IWebHost webHost)
  {
    this.webHost = webHost ?? throw new ArgumentNullException(nameof(webHost));
    this.logger = this.webHost.Services.GetRequiredService<ILogger<WebHostAdapter>>();
  }

  #endregion

  #region IHostedService

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    ObjectDisposedException.ThrowIf(this.disposed, this);

    await this.webHost.StartAsync(cancellationToken);

    var addresses = this.webHost.ServerFeatures.Get<IServerAddressesFeature>();
    if (addresses?.Addresses != null)
    {
      foreach (var address in addresses.Addresses)
        this.logger.LogInformation("Stride Explorer is listening on {Address}", address);
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    ObjectDisposedException.ThrowIf(this.disposed, this);

    await this.webHost.StopAsync(cancellationToken);
  }

  #endregion

  #region IDisposable

  public void Dispose()
  {
    if (this.disposed)
      return;

    this.disposed = true;
    this.webHost.Dispose();
  }

  #endregion
}
