using System;
using System.Collections.Generic;

namespace StrideEnchanted.Explorer.Interfaces;

public interface ITrackedEntityComponent : IDisposable
{
  Guid Id { get; }

  string Name { get; }

  IEnumerable<ITrackedEntityComponentParameter> Parameters { get; }
}
