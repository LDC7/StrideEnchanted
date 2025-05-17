using System.Threading.Tasks;
using Stride.Engine;

namespace StrideEnchanted.Example;

public class StringParametersScript : AsyncScript
{
  public string StringFieldValue;
  public string StringPropertyValue { get; set; }

  public override async Task Execute()
  {
    while (this.Game.IsRunning && !this.CancellationToken.IsCancellationRequested)
    {
      await this.Script.NextFrame();
    }
  }
}
