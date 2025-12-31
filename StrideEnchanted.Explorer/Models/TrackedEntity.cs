using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stride.Engine;
using StrideEnchanted.Explorer.Interfaces;
using StrideEnchanted.Explorer.Services;

namespace StrideEnchanted.Explorer.Models;

internal sealed class TrackedEntity : ITrackedEntity
{
  #region Fields and Properties

  private static readonly PropertyChangedEventArgs namePropertyChangedEventArgs = new(nameof(ITrackedEntity.Name));
  private static readonly PropertyChangedEventArgs childrenPropertyChangedEventArgs = new(nameof(ITrackedEntity.Children));
  private static readonly PropertyChangedEventArgs componentsPropertyChangedEventArgs = new(nameof(ITrackedEntity.Components));

  private readonly DataTrackingTimer dataTrackingTimer;
  private readonly ILoggerFactory loggerFactory;
  private readonly ILogger<TrackedEntity> logger;

  private readonly Lazy<List<TrackedEntity>> lazyChildren;
  private readonly Lazy<List<TrackedEntityComponent>> lazyComponents;

  public Entity Entity { get; }

  #endregion

  #region Constructor

  public TrackedEntity(
    Entity entity,
    DataTrackingTimer dataTrackingTimer,
    ILoggerFactory loggerFactory)
  {
    this.loggerFactory = loggerFactory;
    this.logger = this.loggerFactory.CreateLogger<TrackedEntity>();
    this.Entity = entity;
    this.Name = entity.Name;
    this.dataTrackingTimer = dataTrackingTimer;

    this.lazyChildren = new Lazy<List<TrackedEntity>>(this.CreateChildren);
    this.lazyComponents = new Lazy<List<TrackedEntityComponent>>(this.CreateComponents);

    this.dataTrackingTimer.Subscribe(this.Invalidate);

    this.logger.LogTrace("Created {id} for {name}", this.Id, this.Entity.Name);
  }

  #endregion

  #region Methods

  private List<TrackedEntity> CreateChildren()
  {
    return this.Entity.GetChildren()
      .Select(c => new TrackedEntity(c, this.dataTrackingTimer, this.loggerFactory))
      .ToList();
  }

  private List<TrackedEntityComponent> CreateComponents()
  {
    return this.Entity.Components
      .Select(c => new TrackedEntityComponent(c, this.dataTrackingTimer, this.loggerFactory))
      .ToList();
  }

  private async ValueTask Invalidate()
  {
    if (this.Name != this.Entity.Name)
    {
      this.Name = this.Entity.Name;
      this.PropertyChanged?.Invoke(this, namePropertyChangedEventArgs);
    }

    if (this.lazyChildren.IsValueCreated)
      await this.InvalidateChildren();

    if (this.lazyComponents.IsValueCreated)
      await this.InvalidateComponents();
  }

  private ValueTask InvalidateChildren()
  {
    var children = this.lazyChildren.Value;
    var actual = this.Entity.GetChildren().ToImmutableArray();
    var added = actual
      .Where(e => !children.Any(c => c.Entity == e))
      .ToImmutableArray();
    var removed = children
      .Where(c => !actual.Any(e => c.Entity == e))
      .ToImmutableArray();

    if (!added.Any() && !removed.Any())
      return new ValueTask();

    foreach (var removedChild in removed)
    {
      children.Remove(removedChild);
      removedChild.Dispose();
    }

    foreach (var addedChild in added)
      children.Add(new TrackedEntity(addedChild, this.dataTrackingTimer, this.loggerFactory));

    this.PropertyChanged?.Invoke(this, childrenPropertyChangedEventArgs);
    return new ValueTask();
  }

  private ValueTask InvalidateComponents()
  {
    var components = this.lazyComponents.Value;
    var actual = this.Entity.Components.ToImmutableArray();
    var added = actual
      .Where(e => !components.Any(c => c.Component == e))
      .ToImmutableArray();
    var removed = components
      .Where(c => !actual.Any(e => c.Component == e))
      .ToImmutableArray();

    if (!added.Any() && !removed.Any())
      return new ValueTask();

    foreach (var removedComponent in removed)
    {
      components.Remove(removedComponent);
      removedComponent.Dispose();
    }

    foreach (var addedComponent in added)
      components.Add(new TrackedEntityComponent(addedComponent, this.dataTrackingTimer, this.loggerFactory));

    this.PropertyChanged?.Invoke(this, componentsPropertyChangedEventArgs);
    return new ValueTask();
  }

  #endregion

  #region ITrackedEntity

  public Guid Id => this.Entity.Id;

  public string Name { get; private set; }

  public IReadOnlyCollection<ITrackedEntity> Children => this.lazyChildren.Value;

  public IReadOnlyCollection<ITrackedEntityComponent> Components => this.lazyComponents.Value;

  public event PropertyChangedEventHandler? PropertyChanged;

  public void Dispose()
  {
    this.logger.LogTrace("Dispose {id} for {name}", this.Id, this.Entity.Name);

    this.dataTrackingTimer.Unsubscribe(this.Invalidate);

    if (this.lazyChildren.IsValueCreated)
    {
      foreach (var child in this.lazyChildren.Value)
        child.Dispose();
    }

    if (this.lazyComponents.IsValueCreated)
    {
      foreach (var component in this.lazyComponents.Value)
        component.Dispose();
    }
  }

  #endregion
}
