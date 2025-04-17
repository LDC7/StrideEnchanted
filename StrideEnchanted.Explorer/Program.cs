using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using StrideEnchanted.Explorer.Components;

namespace StrideEnchanted.Explorer;

#warning TODO: нужно реализовать liveexplorer дерева сцены.
public class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddMudServices();

    builder.Services
      .AddRazorComponents()
      .AddInteractiveServerComponents();

    var app = builder.Build();

    app.UseExceptionHandler("/Error");

    app.UseStaticFiles();
    app.UseAntiforgery();

    app
      .MapRazorComponents<App>()
      .AddInteractiveServerRenderMode();

    await app.RunAsync();
  }
}
