using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using StrideEnchanted.Explorer.Options;
using StrideEnchanted.Explorer.Services;
using Xunit;

namespace StrideEnchanted.Explorer.Tests;

public sealed class DataTrackingTimerTests
{
  [Fact]
  public async Task RunOnceAsync_InvokesSubscribedActions()
  {
    var options = CreateOptionsMonitor(new StrideExplorerOptions());
    var timer = new DataTrackingTimer(options.Object, TimeProvider.System);

    var called = 0;
    ValueTask Handler()
    {
      called++;
      return ValueTask.CompletedTask;
    }

    timer.Subscribe(Handler);

    await timer.RunSubscribedActions(default);

    Assert.Equal(1, called);

    timer.Unsubscribe(Handler);
    await timer.RunSubscribedActions(default);

    Assert.Equal(1, called);
  }

  [Fact]
  public void Dispose_CancelsLoopGracefully()
  {
    var options = CreateOptionsMonitor(new StrideExplorerOptions
    {
      DataTrackingTimer = TimeSpan.FromMilliseconds(5)
    });

    var timer = new DataTrackingTimer(options.Object, TimeProvider.System);

    var dispose = new Action(timer.Dispose);

    var exception = Record.Exception(dispose);

    Assert.Null(exception);
  }

  private static Mock<IOptionsMonitor<StrideExplorerOptions>> CreateOptionsMonitor(StrideExplorerOptions options)
  {
    var mock = new Mock<IOptionsMonitor<StrideExplorerOptions>>();
    mock.SetupGet(o => o.CurrentValue).Returns(options);
    mock.Setup(o => o.Get(It.IsAny<string?>())).Returns(options);
    return mock;
  }
}
