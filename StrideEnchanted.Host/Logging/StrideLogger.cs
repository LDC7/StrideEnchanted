using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Stride.Core.Diagnostics;

namespace StrideEnchanted.Host.Logging;

internal sealed class StrideLogger : Microsoft.Extensions.Logging.ILogger
{
  #region Fields and Properties

  private readonly Logger strideLogger;
  private readonly IExternalScopeProvider scopeProvider;

  #endregion

  #region Constructor

  public StrideLogger(string categoryName, IExternalScopeProvider scopeProvider)
  {
    ArgumentNullException.ThrowIfNull(categoryName);
    ArgumentNullException.ThrowIfNull(scopeProvider);

    this.strideLogger = GlobalLogger.GetLogger(categoryName, LogMessageType.Debug);
    this.scopeProvider = scopeProvider;
  }

  #endregion

  #region ILogger

  public IDisposable? BeginScope<TState>(TState state)
    where TState : notnull
  {
    return this.scopeProvider.Push(state);
  }

  public bool IsEnabled(LogLevel logLevel)
  {
    return logLevel != LogLevel.None;
  }

  public void Log<TState>(
    LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter)
  {
    if (!this.IsEnabled(logLevel))
      return;

    ArgumentNullException.ThrowIfNull(formatter);

    var message = this.BuildMessage(state, exception, formatter);
    this.Write(logLevel, message);
  }

  #endregion

  #region Methods

  private string BuildMessage<TState>(TState state, Exception? exception, Func<TState, Exception?, string> formatter)
  {
    var message = formatter(state, exception);
    if (exception != null)
      message = $"{message}{Environment.NewLine}{exception}";

    var scopeBuilder = new ScopeBuilder(message);
    this.scopeProvider.ForEachScope(static (scope, sb) => sb.Append($"[{scope}] "), scopeBuilder);

    return scopeBuilder.ToString();
  }

  private void Write(LogLevel logLevel, string message)
  {
    switch (logLevel)
    {
      case LogLevel.Critical:
        this.strideLogger.Fatal(message);
        break;
      case LogLevel.Error:
        this.strideLogger.Error(message);
        break;
      case LogLevel.Warning:
        this.strideLogger.Warning(message);
        break;
      case LogLevel.Information:
        this.strideLogger.Info(message);
        break;
      case LogLevel.Debug:
        this.strideLogger.Debug(message);
        break;
      case LogLevel.Trace:
        this.strideLogger.Verbose(message);
        break;
      default:
        this.strideLogger.Info(message);
        break;
    }
  }

  #endregion

  #region Nested

  private sealed class ScopeBuilder
  {
    private readonly StringBuilder stringBuilder = new();
    private readonly string baseMessage;

    public ScopeBuilder(string baseMessage)
    {
      this.baseMessage = baseMessage;
    }

    public void Append(string text)
    {
      this.stringBuilder.Append(text);
    }

    public override string ToString()
    {
      return this.stringBuilder.Append(this.baseMessage).ToString();
    }
  }

  #endregion
}
