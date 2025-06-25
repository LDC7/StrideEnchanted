using System;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Stride.Engine;
using StrideEnchanted.Explorer.Services;

namespace StrideEnchanted.Explorer.Models;

internal sealed class TrackedSceneInstance : INotifyPropertyChanged, IDisposable
{
  #region Fields and Properties

  private static readonly PropertyChangedEventArgs rootScenePropertyChangedEventArgs = new(nameof(RootScene));

  private readonly SceneInstance sceneInstance;
  private readonly DataTrackingTimer dataTrackingTimer;
  private readonly ILoggerFactory loggerFactory;
  private readonly ILogger<TrackedSceneInstance> logger;

  private Lazy<TrackedScene?> lazyRootScene;

  public TrackedScene? RootScene => this.lazyRootScene.Value;

  #endregion

  #region Constructor

  public TrackedSceneInstance(
    SceneInstance sceneInstance,
    DataTrackingTimer dataTrackingTimer,
    ILoggerFactory loggerFactory)
  {
    this.loggerFactory = loggerFactory;
    this.logger = this.loggerFactory.CreateLogger<TrackedSceneInstance>();
    this.sceneInstance = sceneInstance;
    this.dataTrackingTimer = dataTrackingTimer;

    this.lazyRootScene = new Lazy<TrackedScene?>(this.CreateRootScene);
    this.sceneInstance.RootSceneChanged += this.HandleRootSceneChanged;

    this.logger.LogTrace("Created for {name}", this.sceneInstance.Name);
  }

  #endregion

  #region Methods

  private TrackedScene? CreateRootScene()
  {
    return this.sceneInstance.RootScene != null
      ? new TrackedScene(this.sceneInstance.RootScene, this.dataTrackingTimer, this.loggerFactory)
      : null;
  }

  private void HandleRootSceneChanged(object? sender, EventArgs e)
  {
    if (!this.lazyRootScene.IsValueCreated)
      return;

    if (this.sceneInstance.RootScene != this.RootScene?.Scene)
    {
      this.RootScene?.Dispose();
      this.lazyRootScene = new Lazy<TrackedScene?>(this.CreateRootScene);

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
    this.logger.LogTrace("Dispose for {name}", this.sceneInstance.Name);
    this.sceneInstance.RootSceneChanged -= this.HandleRootSceneChanged;
    this.RootScene?.Dispose();
  }

  #endregion
}
