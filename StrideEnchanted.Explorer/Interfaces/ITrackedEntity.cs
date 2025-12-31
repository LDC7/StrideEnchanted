using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StrideEnchanted.Explorer.Interfaces;

public interface ITrackedEntity : INotifyPropertyChanged, IDisposable
{
  Guid Id { get; }

  string Name { get; }

  IReadOnlyCollection<ITrackedEntity> Children { get; }

  IReadOnlyCollection<ITrackedEntityComponent> Components { get; }
}
