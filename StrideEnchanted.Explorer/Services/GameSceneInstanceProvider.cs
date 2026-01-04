using Stride.Core;
using Stride.Engine;
using Stride.Games;
using StrideEnchanted.Explorer.Interfaces;

namespace StrideEnchanted.Explorer.Services;

internal sealed class GameSceneInstanceProvider : ISceneInstanceProvider
{
  #region Fields and Properties

  private readonly IGame game;

  public bool IsGameRunning => this.game.IsRunning;

  #endregion

  #region Constructor

  public GameSceneInstanceProvider(IGame game)
  {
    this.game = game;
  }

  #endregion

  #region ISceneInstanceProvider

  public SceneInstance GetSceneInstance()
  {
    var sceneSystem = this.game.Services.GetSafeServiceAs<SceneSystem>()!;
    return sceneSystem.SceneInstance;
  }

  #endregion
}
