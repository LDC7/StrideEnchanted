using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using StrideEnchanted.Explorer.Options;

namespace StrideEnchanted.Explorer.Services;

internal sealed class DataTrackingTimer : IDisposable
{
  #region Fields and Properties

  private readonly ReaderWriterLockSlim locker = new();
  private readonly CancellationTokenSource cancellationTokenSource = new();
  private readonly IOptionsMonitor<StrideExplorerOptions> optionsMonitor;
  private readonly Task timerLoopTask;
  private readonly List<Func<ValueTask>> subscriptions = new();

  #endregion

  #region Constructor

  public DataTrackingTimer(IOptionsMonitor<StrideExplorerOptions> optionsMonitor)
  {
    this.optionsMonitor = optionsMonitor;
    this.timerLoopTask = this.TimerLoop(this.cancellationTokenSource.Token);
  }

  #endregion

  #region Methods

  private async Task TimerLoop(CancellationToken cancellationToken)
  {
    while (true)
    {
      if (cancellationToken.IsCancellationRequested)
        break;

      ImmutableArray<Func<ValueTask>> actions;
      this.locker.EnterReadLock();
      try
      {
        actions = this.subscriptions.ToImmutableArray();
      }
      finally
      {
        this.locker.ExitReadLock();
      }

      foreach (var action in actions)
      {
        if (cancellationToken.IsCancellationRequested)
          break;

        _ = action.Invoke();
      }

      if (cancellationToken.IsCancellationRequested)
        break;

      await Task.Delay(this.optionsMonitor.CurrentValue.DataTrackingTimer, CancellationToken.None);
    }
  }

  public void Subscribe(Func<ValueTask> action)
  {
    this.locker.EnterWriteLock();
    try
    {
      this.subscriptions.Add(action);
    }
    finally
    {
      this.locker.ExitWriteLock();
    }
  }

  public void Unsubscribe(Func<ValueTask> action)
  {
    this.locker.EnterWriteLock();
    try
    {
      this.subscriptions.Remove(action);
    }
    finally
    {
      this.locker.ExitWriteLock();
    }
  }

  #endregion

  #region IDisposable

  public void Dispose()
  {
    this.cancellationTokenSource.Cancel();
  }

  #endregion
}
