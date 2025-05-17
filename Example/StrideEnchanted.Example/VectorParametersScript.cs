using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace StrideEnchanted.Example;

public class VectorParametersScript : AsyncScript
{
  public Vector2 Vector2FieldValue;
  public Vector2? Vector2NullableFieldValue;
  public Vector2 Vector2PropertyValue { get; set; }
  public Vector2? Vector2NullablePropertyValue { get; set; }


  public Vector3 Vector3FieldValue;
  public Vector3? Vector3NullableFieldValue;
  public Vector3 Vector3PropertyValue { get; set; }
  public Vector3? Vector3NullablePropertyValue { get; set; }


  public Vector4 Vector4FieldValue;
  public Vector4? Vector4NullableFieldValue;
  public Vector4 Vector4PropertyValue { get; set; }
  public Vector4? Vector4NullablePropertyValue { get; set; }


  public Quaternion QuaternionFieldValue;
  public Quaternion? QuaternionNullableFieldValue;
  public Quaternion QuaternionPropertyValue { get; set; }
  public Quaternion? QuaternionNullablePropertyValue { get; set; }

  public override async Task Execute()
  {
    while (this.Game.IsRunning && !this.CancellationToken.IsCancellationRequested)
    {
      await this.Script.NextFrame();
    }
  }
}
