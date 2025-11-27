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
  private readonly ILogger<IHostedService> logger;

  #endregion

  #region Constructor

  public WebHostAdapter(IWebHost webHost)
  {
    this.webHost = webHost;
    this.logger = this.webHost.Services.GetRequiredService<ILogger<IHostedService>>();
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
