using System;

namespace StrideEnchanted.Example.Services;

public sealed class TestService
{
  public Guid TestGuid { get; } = Guid.NewGuid();
}
