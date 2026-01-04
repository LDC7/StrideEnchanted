using Microsoft.Extensions.Logging;
using StrideEnchanted.Host.Logging;
using Xunit;

namespace StrideEnchanted.Host.Tests;

public sealed class StrideLoggerTests
{
  [Theory]
  [InlineData(LogLevel.Trace)]
  [InlineData(LogLevel.Debug)]
  [InlineData(LogLevel.Information)]
  [InlineData(LogLevel.Warning)]
  [InlineData(LogLevel.Error)]
  [InlineData(LogLevel.Critical)]
  public void Log_DoesNotThrow_ForEnabledLevels(LogLevel level)
  {
    var scopeProvider = new LoggerExternalScopeProvider();
    var logger = new StrideLogger("Test", scopeProvider);

    var exception = Record.Exception(() =>
      logger.Log(level, default, "hello", null, static (state, ex) => state));

    Assert.Null(exception);
  }

  [Fact]
  public void BeginScope_PushesState()
  {
    var scopeProvider = new LoggerExternalScopeProvider();
    var logger = new StrideLogger("Test", scopeProvider);

    using var scope = logger.BeginScope("scope-state");

    Assert.NotNull(scope);
  }
}
