using System;
using System.Threading.Tasks;
using Stride.Engine;

namespace StrideEnchanted.Example;

public class TestPropertyChangingScript : AsyncScript
{
  private Task delayTask = Task.CompletedTask;
  public int RandomFieldValue;
  public int RandomPropertyValue { get; set; }

  public override async Task Execute()
  {
    while (this.Game.IsRunning && !this.CancellationToken.IsCancellationRequested)
    {
      if (this.delayTask.IsCompleted)
      {
        this.RandomFieldValue = Guid.NewGuid().GetHashCode();
        this.RandomPropertyValue = Guid.NewGuid().GetHashCode();
        this.delayTask = Task.Delay(TimeSpan.FromSeconds(5));
      }

      await this.Script.NextFrame();
    }
  }
}
