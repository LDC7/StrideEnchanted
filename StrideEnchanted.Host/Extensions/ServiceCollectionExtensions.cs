using System;
using Microsoft.Extensions.DependencyInjection;
using Stride.Core;
using Stride.Games;

namespace StrideEnchanted.Host.Extensions;

internal static class ServiceCollectionExtensions
{
  public static IServiceCollection AddGameService<T>(this IServiceCollection services)
    where T : class
  {
    ArgumentNullException.ThrowIfNull(services);

    return services.AddSingleton(sp =>
    {
      var gameServices = sp.GetRequiredService<IGame>().Services;
      return gameServices.GetSafeServiceAs<T>();
    });
  }
}
