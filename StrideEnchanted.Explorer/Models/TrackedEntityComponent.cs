using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Stride.Core;
using Stride.Engine;
using StrideEnchanted.Explorer.Interfaces;
using StrideEnchanted.Explorer.Services;

namespace StrideEnchanted.Explorer.Models;

internal sealed class TrackedEntityComponent : ITrackedEntityComponent
{
  #region Fields and Properties

  private readonly ILoggerFactory loggerFactory;
  private readonly ILogger<TrackedEntityComponent> logger;
  private readonly DataTrackingTimer dataTrackingTimer;
  private readonly Type componentType;

  private readonly Lazy<TrackedEntityComponentParameter[]> lazyParameters;

  public EntityComponent Component { get; }

  #endregion

  #region TrackedEntityComponent

  public TrackedEntityComponent(
    EntityComponent component,
    DataTrackingTimer dataTrackingTimer,
    ILoggerFactory loggerFactory)
  {
    this.loggerFactory = loggerFactory;
    this.logger = this.loggerFactory.CreateLogger<TrackedEntityComponent>();
    this.dataTrackingTimer = dataTrackingTimer;
    this.Component = component;
    this.componentType = this.Component.GetType();

    this.lazyParameters = new Lazy<TrackedEntityComponentParameter[]>(this.CreateParameters);

    this.logger.LogTrace("Created {id}", this.Id);
  }

  #endregion

  #region Methods

  private TrackedEntityComponentParameter[] CreateParameters()
  {
    var fields = this.componentType
      .GetFields(BindingFlags.Public | BindingFlags.Instance)
      .Where(f => !f.IsInitOnly)
      .Select(f => new TrackedEntityComponentParameter(this, f, this.dataTrackingTimer, this.loggerFactory));
    var properties = this.componentType
      .GetProperties(BindingFlags.Public | BindingFlags.Instance)
      .Where(p => p.CanRead && p.CanWrite)
      .Select(p => new TrackedEntityComponentParameter(this, p, this.dataTrackingTimer, this.loggerFactory));

    return fields.Concat(properties)
      .Where(p => p.MemberInfo.GetCustomAttribute<DataMemberIgnoreAttribute>() == null)
      .Where(p => p.MemberInfo.GetCustomAttribute<DisplayAttribute>()?.Browsable != false)
      .OrderBy(p => p.Name)
      .ToArray();
  }

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

  public IEnumerable<ITrackedEntityComponentParameter> Parameters => this.lazyParameters.Value;

  public void Dispose()
  {
    this.logger.LogTrace("Dispose {id}", this.Id);

    if (this.lazyParameters.IsValueCreated)
    {
      foreach (var parameter in this.lazyParameters.Value)
        parameter.Dispose();
    }
  }

  #endregion
}
