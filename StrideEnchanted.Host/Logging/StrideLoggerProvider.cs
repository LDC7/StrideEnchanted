using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace StrideEnchanted.Host.Logging;

internal sealed class StrideLoggerProvider : ILoggerProvider, ISupportExternalScope
{
  #region Fields and Properties

  private readonly ConcurrentDictionary<string, StrideLogger> loggers = new();
  private IExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();
  private bool disposed;

  #endregion

  #region ILoggerProvider

  public ILogger CreateLogger(string categoryName)
  {
    if (this.disposed)
      throw new ObjectDisposedException(nameof(StrideLoggerProvider));

    return this.loggers.GetOrAdd(categoryName, name => new StrideLogger(name, this.scopeProvider));
  }

  public void Dispose()
  {
    if (this.disposed)
      return;

    this.disposed = true;
    this.loggers.Clear();
  }

  #endregion

  #region ISupportExternalScope

  public void SetScopeProvider(IExternalScopeProvider scopeProvider)
  {
    if (this.disposed)
      throw new ObjectDisposedException(nameof(StrideLoggerProvider));

    this.scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
  }

  #endregion
}
