using Microsoft.Extensions.Hosting;

namespace StrideEnchanted.Host;

public interface IStrideApplicationBuilder : IHostApplicationBuilder
{
  StrideApplication Build();
}
