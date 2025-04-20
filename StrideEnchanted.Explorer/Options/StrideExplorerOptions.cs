using System;

namespace StrideEnchanted.Explorer.Options;

public sealed class StrideExplorerOptions
{
  public TimeSpan DataTrackingTimer { get; set; } = TimeSpan.FromSeconds(1);
}
