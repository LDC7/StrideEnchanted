using System;
using Stride.Core;

namespace Stride.Engine;

public static class ServiceRegistryExtensions
{
  public static T? Resolve<T>(this IServiceRegistry registry)
    where T : class
  {
    return registry.GetService<T>() ?? Resolve(registry, typeof(T)) as T;
  }

  public static T Require<T>(this IServiceRegistry registry)
    where T : class
  {
    return Resolve<T>(registry)
      ?? throw new InvalidOperationException($"Service {typeof(T)} not found in DI.");
  }

  public static object? Resolve(this IServiceRegistry registry, Type type)
  {
    var hostServiceProvider = registry.GetSafeServiceAs<IServiceProvider>();
    return hostServiceProvider.GetService(type);
  }
}
