using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Stride.Core;
using Stride.Engine;
using StrideEnchanted.Explorer.Interfaces;
using StrideEnchanted.Explorer.Services;

namespace StrideEnchanted.Explorer.Models;

internal sealed class TrackedEntityComponent : ITrackedEntityComponent
{
  #region Fields and Properties

  private readonly Type componentType;
  private readonly ImmutableDictionary<string, TrackedEntityComponentParameter> parameters;

  public EntityComponent Component { get; }

  #endregion

  #region TrackedEntityComponent

  public TrackedEntityComponent(EntityComponent component, DataTrackingTimer dataTrackingTimer)
  {
    this.Component = component;
    this.componentType = this.Component.GetType();

    var fields = this.componentType
      .GetFields(BindingFlags.Public | BindingFlags.Instance)
      .Where(f => !f.IsInitOnly)
      .Select(f => new TrackedEntityComponentParameter(this, f, dataTrackingTimer));
    var properties = this.componentType
      .GetProperties(BindingFlags.Public | BindingFlags.Instance)
      .Where(p => p.CanRead && p.CanWrite)
      .Select(p => new TrackedEntityComponentParameter(this, p, dataTrackingTimer));

    this.parameters = fields.Concat(properties)
      .Where(p => p.MemberInfo.GetCustomAttribute<DataMemberIgnoreAttribute>() == null)
      .Where(p => p.MemberInfo.GetCustomAttribute<DisplayAttribute>()?.Browsable != false)
      .ToImmutableDictionary(p => p.Name, p => p);
  }

  #endregion

  #region Methods

  internal object? GetValue(TrackedEntityComponentParameter parameter)
  {
    if (parameter.MemberInfo is FieldInfo fieldInfo)
      return fieldInfo.GetValue(this.Component);

    if (parameter.MemberInfo is PropertyInfo propertyInfo)
      return propertyInfo.GetValue(this.Component);

    throw new NotSupportedException("Unknown parameter type.");
  }

  internal void SetValue(TrackedEntityComponentParameter parameter, object? value)
  {
    if (parameter.MemberInfo is FieldInfo fieldInfo)
      fieldInfo.SetValue(this.Component, value);

    if (parameter.MemberInfo is PropertyInfo propertyInfo)
      propertyInfo.SetValue(this.Component, value);
  }

  #endregion

  #region ITrackedEntityComponent

  public Guid Id => this.Component.Id;

  public string Name => this.componentType.Name;

  public IEnumerable<ITrackedEntityComponentParameter> Parameters => this.parameters.Values;

  public void Dispose()
  {
    foreach (var pair in this.parameters)
      pair.Value.Dispose();
  }

  #endregion
}
