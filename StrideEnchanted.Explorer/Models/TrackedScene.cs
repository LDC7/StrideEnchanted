using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Stride.Core.Collections;
using Stride.Engine;
using StrideEnchanted.Explorer.Interfaces;
using StrideEnchanted.Explorer.Services;

namespace StrideEnchanted.Explorer.Models;

internal sealed class TrackedScene : ITrackedScene
{
  #region Fields and Properties

  private static readonly PropertyChangedEventArgs childrenPropertyChangedEventArgs = new(nameof(ITrackedScene.Children));
  private static readonly PropertyChangedEventArgs entitiesPropertyChangedEventArgs = new(nameof(ITrackedScene.Entities));

  private readonly DataTrackingTimer dataTrackingTimer;
  private readonly ReaderWriterLockSlim childrenLock = new();
  private readonly List<TrackedScene> children;
  private readonly ReaderWriterLockSlim entitiesLock = new();
  private readonly List<TrackedEntity> entities;

  public Scene Scene { get; }

  #endregion

  #region Constructor

  public TrackedScene(Scene scene, DataTrackingTimer dataTrackingTimer)
  {
    this.Scene = scene;
    this.dataTrackingTimer = dataTrackingTimer;

    this.Scene.Children.CollectionChanged += this.HandleChildScenesChanged;
    this.children = this.Scene.Children
      .Select(s => new TrackedScene(s, this.dataTrackingTimer))
      .ToList();

    this.Scene.Entities.CollectionChanged += this.HandleEntitiesChanged;
    this.entities = this.Scene.Entities
      .Select(e => new TrackedEntity(e, this.dataTrackingTimer))
      .ToList();
  }

  #endregion

  #region Methods

  private void HandleChildScenesChanged(object? sender, TrackingCollectionChangedEventArgs args)
  {
    if (args.Item is Scene scene)
    {
      this.childrenLock.EnterWriteLock();
      try
      {
        if (args.Action == NotifyCollectionChangedAction.Add)
          this.children.Add(new TrackedScene(scene, this.dataTrackingTimer));

        if (args.Action != NotifyCollectionChangedAction.Remove)
        {
          var removedScene = this.children.First(s => s.Scene == scene);
          this.children.Remove(removedScene);
          removedScene.Dispose();
        }

        this.PropertyChanged?.Invoke(this, childrenPropertyChangedEventArgs);
      }
      finally
      {
        this.childrenLock.ExitWriteLock();
      }
    }
  }

  private void HandleEntitiesChanged(object? sender, TrackingCollectionChangedEventArgs args)
  {
    if (args.Item is Entity entity)
    {
      this.entitiesLock.EnterWriteLock();
      try
      {
        if (args.Action == NotifyCollectionChangedAction.Add)
          this.entities.Add(new TrackedEntity(entity, this.dataTrackingTimer));

        if (args.Action != NotifyCollectionChangedAction.Remove)
        {
          var removedEntity = this.entities.First(e => e.Entity == entity);
          this.entities.Remove(removedEntity);
          removedEntity.Dispose();
        }

        this.PropertyChanged?.Invoke(this, entitiesPropertyChangedEventArgs);
      }
      finally
      {
        this.entitiesLock.ExitWriteLock();
      }
    }
  }

  #endregion

  #region ITrackedScene

  public Guid Id => this.Scene.Id;

  public string Name => this.Scene.Name;

  public IReadOnlyCollection<ITrackedScene> Children => this.children;

  public IReadOnlyCollection<ITrackedEntity> Entities => this.entities;

  public event PropertyChangedEventHandler? PropertyChanged;

  public void Dispose()
  {
    this.Scene.Children.CollectionChanged -= this.HandleChildScenesChanged;
    this.Scene.Entities.CollectionChanged -= this.HandleEntitiesChanged;

    foreach (var child in this.children)
      child.Dispose();
    foreach (var entity in this.entities)
      entity.Dispose();
  }

  #endregion
}
