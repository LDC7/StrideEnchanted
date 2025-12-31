using Microsoft.Extensions.DependencyInjection;
using Stride.Core;
using Stride.Games;

namespace StrideEnchanted.Host.Extensions;

internal static class ServiceCollectionExtensions
{
  public static IServiceCollection AddGameService<T>(this IServiceCollection services)
    where T : class
  {
    return services.AddSingleton(sp => sp.GetRequiredService<IGame>().Services.GetSafeServiceAs<T>());
  }
}
