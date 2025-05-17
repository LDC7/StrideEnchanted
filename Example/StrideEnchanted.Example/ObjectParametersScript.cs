using System.Threading.Tasks;
using Stride.Engine;

namespace StrideEnchanted.Example;

public class ObjectParametersScript : AsyncScript
{
  public object ObjectFieldValue;
  public object ObjectPropertyValue { get; set; }

  public override async Task Execute()
  {
    while (this.Game.IsRunning && !this.CancellationToken.IsCancellationRequested)
    {
      await this.Script.NextFrame();
    }
  }
}
