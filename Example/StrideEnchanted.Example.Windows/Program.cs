using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stride.Engine;
using StrideEnchanted.Example.Services;
using StrideEnchanted.Explorer;
using StrideEnchanted.Host;

namespace StrideEnchanted.Example.Windows;

public sealed class Program
{
  public static async Task Main(string[] args)
  {
    var builder = StrideApplication.CreateBuilder<Game>(args);

    builder.Services.AddScoped<TestService>();

#if DEBUG
    builder.AddStrideExplorer();
#endif

    using var application = builder.Build();

    await application.RunAsync();
  }
}
