using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StrideEnchanted.Explorer;

internal sealed class WebHostAdapter : IHostedService, IDisposable
{
  #region Fields and Properties

  private readonly IWebHost webHost;
  private readonly ILogger<IWebHost> logger;

  #endregion

  #region Constructor

  public WebHostAdapter(IWebHost webHost, ILogger<IWebHost> logger)
  {
    this.webHost = webHost;
    this.logger = logger;
  }

  #endregion

  #region IHostedService

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    await this.webHost.StartAsync(cancellationToken);

    var addresses = this.webHost.ServerFeatures.Get<IServerAddressesFeature>();
    if (addresses != null)
    {
      foreach (var address in addresses.Addresses)
      {
        this.logger.LogInformation("Stride Editor now work: {Address}", address);
      }
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    await this.webHost.StopAsync(cancellationToken);
  }

  #endregion

  #region IDisposable

  public void Dispose()
  {
    this.webHost.Dispose();
  }

  #endregion
}
