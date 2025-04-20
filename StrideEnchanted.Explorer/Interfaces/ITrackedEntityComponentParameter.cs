using System;
using System.ComponentModel;

namespace StrideEnchanted.Explorer.Interfaces;

public interface ITrackedEntityComponentParameter : INotifyPropertyChanged, IDisposable
{
  string Name { get; }

  object? Value { get; }

  void SetValue(object value);
}
