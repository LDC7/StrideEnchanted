using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StrideEnchanted.Explorer.Interfaces;
using StrideEnchanted.Explorer.Services;

namespace StrideEnchanted.Explorer.Models;

internal sealed class TrackedEntityComponentParameter : ITrackedEntityComponentParameter
{
  #region Fields and Properties

  private static readonly PropertyChangedEventArgs valuePropertyChangedEventArgs = new(nameof(ITrackedEntityComponentParameter.Value));

  private readonly DataTrackingTimer dataTrackingTimer;
  private readonly ILogger<TrackedEntityComponentParameter> logger;
  private readonly TrackedEntityComponent component;

  private object? cachedValue;

  public MemberInfo MemberInfo { get; }

  #endregion

  #region Constructor

  public TrackedEntityComponentParameter(
    TrackedEntityComponent component,
    PropertyInfo propertyInfo,
    DataTrackingTimer dataTrackingTimer,
    ILoggerFactory loggerFactory)
    : this(component, propertyInfo, dataTrackingTimer, propertyInfo.PropertyType, loggerFactory)
  {
  }

  public TrackedEntityComponentParameter(
    TrackedEntityComponent component,
    FieldInfo fieldInfo,
    DataTrackingTimer dataTrackingTimer,
    ILoggerFactory loggerFactory)
    : this(component, fieldInfo, dataTrackingTimer, fieldInfo.FieldType, loggerFactory)
  {
  }

  private TrackedEntityComponentParameter(
    TrackedEntityComponent component,
    MemberInfo memberInfo,
    DataTrackingTimer dataTrackingTimer,
    Type parameterType,
    ILoggerFactory loggerFactory)
  {
    this.logger = loggerFactory.CreateLogger<TrackedEntityComponentParameter>();
    this.component = component;
    this.MemberInfo = memberInfo;
    this.dataTrackingTimer = dataTrackingTimer;
    this.ParameterType = parameterType;
    this.Id = DeterministicGuids.DeterministicGuid.Create(this.component.Id, this.Name);

    this.cachedValue = this.component.GetValue(this);
    this.dataTrackingTimer.Subscribe(this.Invalidate);

    this.logger.LogTrace("Created {id}", this.Id);
  }

  #endregion

  #region Methods

  private async ValueTask Invalidate()
  {
    var value = this.component.GetValue(this);
    if (this.cachedValue == value)
      return;

    this.cachedValue = value;
    this.PropertyChanged?.Invoke(this, valuePropertyChangedEventArgs);

    await Task.CompletedTask;
  }

  #endregion

  #region ITrackedEntityComponentParameter

  public Guid Id { get; }

  public string Name => this.MemberInfo.Name;

  public object? Value => this.cachedValue;

  public Type ParameterType { get; }

  public event PropertyChangedEventHandler? PropertyChanged;

  public void SetValue(object? value)
  {
    if (this.cachedValue == value)
      return;

    this.cachedValue = value;
    this.component.SetValue(this, value);
    this.PropertyChanged?.Invoke(this, valuePropertyChangedEventArgs);
  }

  public void Dispose()
  {
    this.logger.LogTrace("Dispose {id}", this.Id);

    this.dataTrackingTimer.Unsubscribe(this.Invalidate);
  }

  #endregion
}
