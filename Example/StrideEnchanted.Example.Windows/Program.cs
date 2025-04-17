using System.Threading.Tasks;
using Stride.Engine;
using StrideEnchanted.Explorer;
using StrideEnchanted.Host;

namespace StrideEnchanted.Example.Windows;

public sealed class Program
{
  public static async Task Main(string[] args)
  {
    var builder = StrideApplication.CreateBuilder<Game>(args);

#if DEBUG
    builder.AddStrideExplorer();
#endif

    using var application = builder.Build();

    await application.RunAsync();
  }
}
