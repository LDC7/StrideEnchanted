using Stride.Engine;

namespace StrideEnchanted.Explorer.Interfaces;

public interface ISceneInstanceProvider
{
  bool IsGameRunning { get; }

  SceneInstance GetSceneInstance();
}

