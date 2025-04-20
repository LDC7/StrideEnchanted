using System;
using System.ComponentModel;
using Stride.Engine;
using StrideEnchanted.Explorer.Services;

namespace StrideEnchanted.Explorer.Models;

internal sealed class TrackedSceneInstance : INotifyPropertyChanged, IDisposable
{
  #region Fields and Properties

  private static readonly PropertyChangedEventArgs rootScenePropertyChangedEventArgs = new(nameof(RootScene));

  private readonly SceneInstance sceneInstance;
  private readonly DataTrackingTimer dataTrackingTimer;

  public TrackedScene? RootScene { get; private set; }

  #endregion

  #region Constructor

  public TrackedSceneInstance(SceneInstance sceneInstance, DataTrackingTimer dataTrackingTimer)
  {
    this.sceneInstance = sceneInstance;
    this.dataTrackingTimer = dataTrackingTimer;

    this.sceneInstance.RootSceneChanged += this.HandleRootSceneChanged;
    if (sceneInstance.RootScene != null)
      this.RootScene = new TrackedScene(this.sceneInstance.RootScene, this.dataTrackingTimer);
  }

  #endregion

  #region Methods

  private void HandleRootSceneChanged(object? sender, EventArgs e)
  {
    if (this.sceneInstance.RootScene != this.RootScene?.Scene)
    {
      this.RootScene?.Dispose();

      this.RootScene = this.sceneInstance.RootScene != null
        ? new TrackedScene(this.sceneInstance.RootScene, this.dataTrackingTimer)
        : null;

      this.PropertyChanged?.Invoke(this, rootScenePropertyChangedEventArgs);
    }
  }

  #endregion

  #region INotifyPropertyChanged

  public event PropertyChangedEventHandler? PropertyChanged;

  #endregion

  #region IDisposable

  public void Dispose()
  {
    this.sceneInstance.RootSceneChanged -= this.HandleRootSceneChanged;
    this.RootScene?.Dispose();
  }

  #endregion
}
