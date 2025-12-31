using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StrideEnchanted.Explorer.Interfaces;

public interface ITrackedScene : INotifyPropertyChanged, IDisposable
{
  Guid Id { get; }

  string Name { get; }

  IReadOnlyCollection<ITrackedScene> Children { get; }

  IReadOnlyCollection<ITrackedEntity> Entities { get; }
}
