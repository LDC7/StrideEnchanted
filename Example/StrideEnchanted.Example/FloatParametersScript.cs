using System.Threading.Tasks;
using Stride.Engine;

namespace StrideEnchanted.Example;

public class FloatParametersScript : AsyncScript
{
  public float FloatFieldValue;
  public float? FloatNullableFieldValue;
  public float FloatPropertyValue { get; set; }
  public float? FloatNullablePropertyValue { get; set; }


  public double DoubleFieldValue;
  public double? DoubleNullableFieldValue;
  public double DoublePropertyValue { get; set; }
  public double? DoubleNullablePropertyValue { get; set; }


  public decimal DecimalFieldValue;
  public decimal? DecimalNullableFieldValue;
  public decimal DecimalPropertyValue { get; set; }
  public decimal? DecimalNullablePropertyValue { get; set; }

  public override async Task Execute()
  {
    while (this.Game.IsRunning && !this.CancellationToken.IsCancellationRequested)
    {
      await this.Script.NextFrame();
    }
  }
}
