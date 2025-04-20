using System.ComponentModel;

namespace StrideEnchanted.Explorer.ViewModels;

public interface IEntityComponentParameterViewModel : INotifyPropertyChanged
{
  public string Name { get; }

  public object? Value { get; }

  void SetValue(object value);
}
