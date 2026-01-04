using System;
using StrideEnchanted.Explorer.Options;

namespace StrideEnchanted.Explorer.Interfaces;

public interface IExplorerHostFactory
{
  IExplorerHost CreateHost(IServiceProvider provider, Action<StrideExplorerOptions>? configureOptions);
}
