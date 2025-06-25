using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
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
  private readonly ILoggerFactory loggerFactory;
  private readonly ILogger<TrackedScene> logger;

  private readonly ReaderWriterLockSlim childrenLock = new();
  private readonly Lazy<List<TrackedScene>> lazyChildren;
  private readonly ReaderWriterLockSlim entitiesLock = new();
  private readonly Lazy<List<TrackedEntity>> lazyEntities;

  public Scene Scene { get; }

  #endregion

  #region Constructor

  public TrackedScene(
    Scene scene,
    DataTrackingTimer dataTrackingTimer,
    ILoggerFactory loggerFactory)
  {
    this.loggerFactory = loggerFactory;
    this.logger = this.loggerFactory.CreateLogger<TrackedScene>();
    this.Scene = scene;
    this.dataTrackingTimer = dataTrackingTimer;

    this.lazyChildren = new Lazy<List<TrackedScene>>(this.CreateChildren);
    this.Scene.Children.CollectionChanged += this.HandleChildScenesChanged;

    this.lazyEntities = new Lazy<List<TrackedEntity>>(this.CreateEntities);
    this.Scene.Entities.CollectionChanged += this.HandleEntitiesChanged;

    this.logger.LogTrace("Created {id} for {name}", this.Id, this.Scene.Name);
  }

  #endregion

  #region Methods

  private List<TrackedScene> CreateChildren()
  {
    return this.Scene.Children
      .Select(s => new TrackedScene(s, this.dataTrackingTimer, this.loggerFactory))
      .ToList();
  }

  private void HandleChildScenesChanged(object? sender, TrackingCollectionChangedEventArgs args)
  {
    if (!this.lazyChildren.IsValueCreated)
      return;

    if (args.Item is Scene scene)
    {
      this.childrenLock.EnterWriteLock();
      try
      {
        var children = this.lazyChildren.Value;
        if (args.Action == NotifyCollectionChangedAction.Add)
          children.Add(new TrackedScene(scene, this.dataTrackingTimer, this.loggerFactory));

        if (args.Action == NotifyCollectionChangedAction.Remove)
        {
          var removedScene = children.First(s => s.Scene == scene);
          children.Remove(removedScene);
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

  private List<TrackedEntity> CreateEntities()
  {
    return this.Scene.Entities
      .Select(e => new TrackedEntity(e, this.dataTrackingTimer, this.loggerFactory))
      .ToList();
  }

  private void HandleEntitiesChanged(object? sender, TrackingCollectionChangedEventArgs args)
  {
    if (!this.lazyEntities.IsValueCreated)
      return;

    if (args.Item is Entity entity)
    {
      this.entitiesLock.EnterWriteLock();
      try
      {
        var entities = this.lazyEntities.Value;
        if (args.Action == NotifyCollectionChangedAction.Add)
          entities.Add(new TrackedEntity(entity, this.dataTrackingTimer, this.loggerFactory));

        if (args.Action == NotifyCollectionChangedAction.Remove)
        {
          var removedEntity = entities.First(e => e.Entity == entity);
          entities.Remove(removedEntity);
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

  public IReadOnlyCollection<ITrackedScene> Children => this.lazyChildren.Value;

  public IReadOnlyCollection<ITrackedEntity> Entities => this.lazyEntities.Value;

  public event PropertyChangedEventHandler? PropertyChanged;

  public void Dispose()
  {
    this.logger.LogTrace("Dispose {id} for {name}", this.Id, this.Scene.Name);

    this.Scene.Children.CollectionChanged -= this.HandleChildScenesChanged;
    this.Scene.Entities.CollectionChanged -= this.HandleEntitiesChanged;

    if (this.lazyChildren.IsValueCreated)
    {
      foreach (var child in this.lazyChildren.Value)
        child.Dispose();
    }

    if (this.lazyEntities.IsValueCreated)
    {
      foreach (var entity in this.lazyEntities.Value)
        entity.Dispose();
    }
  }

  #endregion
}
