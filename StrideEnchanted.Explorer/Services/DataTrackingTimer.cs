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
  private readonly TimeProvider timeProvider;
  private readonly PeriodicTimer timer;
  private readonly Task timerLoopTask;
  private readonly List<Func<ValueTask>> subscriptions = [];

  #endregion

  #region Constructor

  public DataTrackingTimer(
    IOptionsMonitor<StrideExplorerOptions> optionsMonitor,
    TimeProvider timeProvider)
  {
    this.optionsMonitor = optionsMonitor;
    this.timeProvider = timeProvider;
    this.timer = new PeriodicTimer(this.optionsMonitor.CurrentValue.DataTrackingTimer, this.timeProvider);
    this.timerLoopTask = this.TimerLoop(this.cancellationTokenSource.Token);
  }

  #endregion

  #region Methods

  private async Task TimerLoop(CancellationToken cancellationToken)
  {
    try
    {
      while (await this.timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
      {
        await this.RunSubscribedActions(cancellationToken).ConfigureAwait(false);
      }
    }
    catch (OperationCanceledException)
    {
      // graceful shutdown
    }
  }

  public async Task RunSubscribedActions(CancellationToken cancellationToken)
  {
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
      cancellationToken.ThrowIfCancellationRequested();
      await action().ConfigureAwait(false);
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

    try
    {
      this.timerLoopTask.GetAwaiter().GetResult();
    }
    catch (OperationCanceledException)
    {
      // ignore
    }
    finally
    {
      this.timer.Dispose();
      this.cancellationTokenSource.Dispose();
    }
  }

  #endregion
}
