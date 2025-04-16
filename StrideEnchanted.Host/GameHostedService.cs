using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stride.Games;

namespace StrideEnchanted.Host;

internal sealed class GameHostedService<TGame> : IHostedService
  where TGame : class, IGame
{
  #region IHostedService

  public Task StartAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  #endregion
}
