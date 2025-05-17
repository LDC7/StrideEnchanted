using System.Threading.Tasks;
using Stride.Engine;

namespace StrideEnchanted.Example;

public class IntParametersScript : AsyncScript
{
  public byte ByteFieldValue;
  public byte? ByteNullableFieldValue;
  public byte BytePropertyValue { get; set; }
  public byte? ByteNullablePropertyValue { get; set; }


  public int IntFieldValue;
  public int? IntNullableFieldValue;
  public int IntPropertyValue { get; set; }
  public int? IntNullablePropertyValue { get; set; }


  public long LongFieldValue;
  public long? LongNullableFieldValue;
  public long LongPropertyValue { get; set; }
  public long? LongNullablePropertyValue { get; set; }

  public override async Task Execute()
  {
    while (this.Game.IsRunning && !this.CancellationToken.IsCancellationRequested)
    {
      await this.Script.NextFrame();
    }
  }
}
