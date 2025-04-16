using System.Threading.Tasks;
using Stride.Engine;
using StrideEnchanted.Host;

namespace StrideEnchanted.Example;

public sealed class Program
{
  public static async Task Main(string[] args)
  {
    var builder = StrideApplication.CreateBuilder<Game>(args);

    using var application = builder.Build();

    await application.RunAsync();
  }
}
