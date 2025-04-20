using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using StrideEnchanted.Explorer.Interfaces;
using StrideEnchanted.Explorer.Services;

namespace StrideEnchanted.Explorer.Models;

internal sealed class TrackedEntityComponentParameter : ITrackedEntityComponentParameter
{
  #region Fields and Properties

  private static readonly PropertyChangedEventArgs valuePropertyChangedEventArgs = new(nameof(ITrackedEntityComponentParameter.Value));

  private readonly DataTrackingTimer dataTrackingTimer;
  private readonly TrackedEntityComponent component;

  private object? cachedValue;

  public MemberInfo MemberInfo { get; }

  #endregion

  #region Constructor

  public TrackedEntityComponentParameter(TrackedEntityComponent component, PropertyInfo propertyInfo, DataTrackingTimer dataTrackingTimer)
    : this(component, propertyInfo, dataTrackingTimer, propertyInfo.PropertyType)
  {
  }

  public TrackedEntityComponentParameter(TrackedEntityComponent component, FieldInfo fieldInfo, DataTrackingTimer dataTrackingTimer)
    : this(component, fieldInfo, dataTrackingTimer, fieldInfo.FieldType)
  {
  }

  private TrackedEntityComponentParameter(TrackedEntityComponent component, MemberInfo memberInfo, DataTrackingTimer dataTrackingTimer, Type parameterType)
  {
    this.component = component;
    this.MemberInfo = memberInfo;
    this.dataTrackingTimer = dataTrackingTimer;
    this.ParameterType = parameterType;

    this.dataTrackingTimer.Subscribe(this.Invalidate);
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
    this.dataTrackingTimer.Unsubscribe(this.Invalidate);
  }

  #endregion
}
