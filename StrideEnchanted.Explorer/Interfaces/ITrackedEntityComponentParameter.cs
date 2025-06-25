using System;
using System.ComponentModel;

namespace StrideEnchanted.Explorer.Interfaces;

public interface ITrackedEntityComponentParameter : INotifyPropertyChanged, IDisposable
{
  Guid Id { get; }

  string Name { get; }

  object? Value { get; }

  Type ParameterType { get; }

  void SetValue(object? value);
}
