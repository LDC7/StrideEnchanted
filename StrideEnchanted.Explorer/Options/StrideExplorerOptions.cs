using System;

namespace StrideEnchanted.Explorer.Options;

public sealed class StrideExplorerOptions
{
  public TimeSpan RefreshViewTime { get; set; } = TimeSpan.FromSeconds(1);
}
