using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
  private readonly List<TrackedEntity> children;
  private readonly List<TrackedEntityComponent> components;

  public Entity Entity { get; }

  #endregion

  #region Constructor

  public TrackedEntity(Entity entity, DataTrackingTimer dataTrackingTimer)
  {
    this.Entity = entity;
    this.Name = entity.Name;
    this.dataTrackingTimer = dataTrackingTimer;

    this.children = this.Entity.GetChildren()
      .Select(c => new TrackedEntity(c, this.dataTrackingTimer))
      .ToList();

    this.components = this.Entity.Components
      .Select(c => new TrackedEntityComponent(c, this.dataTrackingTimer))
      .ToList();

    this.dataTrackingTimer.Subscribe(this.Invalidate);
  }

  #endregion

  #region Methods

  private async ValueTask Invalidate()
  {
    if (this.Name != this.Entity.Name)
    {
      this.Name = this.Entity.Name;
      this.PropertyChanged?.Invoke(this, namePropertyChangedEventArgs);
    }

    await this.InvalidateChildren();
    await this.InvalidateComponents();
  }

  private ValueTask InvalidateChildren()
  {
    var actual = this.Entity.GetChildren().ToImmutableArray();
    var added = actual
      .Where(e => !this.children.Any(c => c.Entity == e))
      .ToImmutableArray();
    var removed = this.children
      .Where(c => !actual.Any(e => c.Entity == e))
      .ToImmutableArray();

    if (!added.Any() && !removed.Any())
      return new ValueTask();

    foreach (var removedChild in removed)
    {
      this.children.Remove(removedChild);
      removedChild.Dispose();
    }

    foreach (var addedChild in added)
      this.children.Add(new TrackedEntity(addedChild, this.dataTrackingTimer));

    this.PropertyChanged?.Invoke(this, childrenPropertyChangedEventArgs);
    return new ValueTask();
  }

  private ValueTask InvalidateComponents()
  {
    var actual = this.Entity.Components.ToImmutableArray();
    var added = actual
      .Where(e => !this.components.Any(c => c.Component == e))
      .ToImmutableArray();
    var removed = this.components
      .Where(c => !actual.Any(e => c.Component == e))
      .ToImmutableArray();

    if (!added.Any() && !removed.Any())
      return new ValueTask();

    foreach (var removedComponent in removed)
    {
      this.components.Remove(removedComponent);
      removedComponent.Dispose();
    }

    foreach (var addedComponent in added)
      this.components.Add(new TrackedEntityComponent(addedComponent, this.dataTrackingTimer));

    this.PropertyChanged?.Invoke(this, componentsPropertyChangedEventArgs);
    return new ValueTask();
  }

  #endregion

  #region ITrackedEntity

  public Guid Id => this.Entity.Id;

  public string Name { get; private set; }

  public IReadOnlyCollection<ITrackedEntity> Children => this.children;

  public IReadOnlyCollection<ITrackedEntityComponent> Components => this.components;

  public event PropertyChangedEventHandler? PropertyChanged;

  public void Dispose()
  {
    this.dataTrackingTimer.Unsubscribe(this.Invalidate);
    foreach (var child in this.children)
      child.Dispose();
    foreach (var component in this.components)
      component.Dispose();
  }

  #endregion
}
