using System;

namespace StrideEnchanted.Explorer.Options;

public sealed class StrideExplorerOptions
{
  public const string OptionsName = "StrideExplorer";

  public TimeSpan DataTrackingTimer { get; set; } = TimeSpan.FromSeconds(1);

  public string IPAddress { get; set; } = "127.0.0.1";

  public int Port { get; set; } = 51891;

  public bool UseHttps { get; set; } = false;
}
