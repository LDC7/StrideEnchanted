using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StrideEnchanted.Explorer.Options;

namespace StrideEnchanted.Explorer.Interfaces;

public interface IExplorerServicesConfigurator
{
  void ConfigureServices(
    IServiceCollection services,
    IServiceProvider hostServices,
    IConfiguration configuration,
    Action<StrideExplorerOptions>? configureOptions);
}

