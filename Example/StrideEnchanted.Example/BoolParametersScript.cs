using System.Threading.Tasks;
using Stride.Engine;

namespace StrideEnchanted.Example;

public class BoolParametersScript : AsyncScript
{
  public bool BoolFieldValue;

  public bool? BoolNullableFieldValue;

  public bool BoolPropertyValue { get; set; }

  public bool? BoolNullablePropertyValue { get; set; }

  public override async Task Execute()
  {
    while (this.Game.IsRunning && !this.CancellationToken.IsCancellationRequested)
    {
      await this.Script.NextFrame();
    }
  }
}
