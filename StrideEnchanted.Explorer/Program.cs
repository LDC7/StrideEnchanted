using System;
using System.Threading.Tasks;

namespace StrideEnchanted.Explorer;

public sealed class Program
{
  public static async Task Main()
  {
    Console.WriteLine($"Use {nameof(StrideApplicationBuilderExtensions)}.{nameof(StrideApplicationBuilderExtensions.AddStrideExplorer)}");
    await Task.CompletedTask;
  }
}
