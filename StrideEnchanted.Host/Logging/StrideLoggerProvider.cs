using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace StrideEnchanted.Host.Logging;

internal sealed class StrideLoggerProvider : ILoggerProvider, ISupportExternalScope
{
  #region Fields and Properties

  private readonly ConcurrentDictionary<string, StrideLogger> loggers = new();
  private IExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

  #endregion

  #region ILoggerProvider

  public ILogger CreateLogger(string categoryName)
  {
    return this.loggers.GetOrAdd(categoryName, name => new StrideLogger(name, this.scopeProvider));
  }

  public void Dispose()
  {
    this.loggers.Clear();
  }

  #endregion

  #region ISupportExternalScope

  public void SetScopeProvider(IExternalScopeProvider scopeProvider)
  {
    this.scopeProvider = scopeProvider;
  }

  #endregion
}
