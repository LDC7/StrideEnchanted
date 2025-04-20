using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Stride.Core;
using Stride.Engine;
using StrideEnchanted.Explorer.Options;

namespace StrideEnchanted.Explorer.ViewModels;

internal sealed class EntityComponentViewModel : IDisposable
{
  #region Fields and Properties

  private readonly CancellationTokenSource cancellationTokenSource = new();
  private readonly Type componentType;
  private readonly Task updateLoopTask;
  private readonly TimeSpan refreshViewTime;
  private readonly ImmutableDictionary<string, ParameterViewModel> componentParameters;

  public EntityComponent Component { get; }

  public string Name => this.componentType.Name;

  public IEnumerable<string> ParameterNames => this.componentParameters.Keys;

  public IEnumerable<IEntityComponentParameterViewModel> Parameters => this.componentParameters.Values;

  public IEntityComponentParameterViewModel this[string parameterName] => this.componentParameters[parameterName];

  #endregion

  #region Constructor

  public EntityComponentViewModel(EntityComponent component, IOptions<StrideExplorerOptions> options)
  {
    this.Component = component;
    this.refreshViewTime = options.Value.RefreshViewTime;
    this.componentType = this.Component.GetType();

    var fields = this.componentType
      .GetFields(BindingFlags.Public | BindingFlags.Instance)
      .Where(f => !f.IsInitOnly)
      .Select(f => new ParameterViewModel(this, f));
    var properties = this.componentType
      .GetProperties(BindingFlags.Public | BindingFlags.Instance)
      .Where(p => p.CanRead && p.CanWrite)
      .Select(p => new ParameterViewModel(this, p));

    this.componentParameters = fields.Concat(properties)
      .Where(p => p.MemberInfo.GetCustomAttribute<DataMemberIgnoreAttribute>() == null)
      .Where(p => p.MemberInfo.GetCustomAttribute<DisplayAttribute>()?.Browsable != false)
      .OrderBy(p => p.Name)
      .ToImmutableDictionary(m => m.Name, m => m);

    this.updateLoopTask = this.UpdateLoop(this.cancellationTokenSource.Token);
  }

  #endregion

  #region Methods

  private async Task UpdateLoop(CancellationToken cancellationToken)
  {
    while (true)
    {
      if (cancellationToken.IsCancellationRequested)
        break;

      foreach (var parameter in this.componentParameters.Values)
        parameter.InvalidateCachedValue();

      await Task.Delay(this.refreshViewTime, CancellationToken.None);
    }
  }

  private object? GetValue(ParameterViewModel parameter)
  {
    if (parameter.MemberInfo is FieldInfo fieldInfo)
      return fieldInfo.GetValue(this.Component);

    if (parameter.MemberInfo is PropertyInfo propertyInfo)
      return propertyInfo.GetValue(this.Component);

    throw new NotSupportedException("Unknown parameter type.");
  }

  private void SetValue(ParameterViewModel parameter, object? value)
  {
    if (parameter.MemberInfo is FieldInfo fieldInfo)
      fieldInfo.SetValue(this.Component, value);

    if (parameter.MemberInfo is PropertyInfo propertyInfo)
      propertyInfo.SetValue(this.Component, value);
  }

  #endregion

  #region IDisposable

  public void Dispose()
  {
    this.cancellationTokenSource.Cancel();
  }

  #endregion

  #region Nested

  internal sealed class ParameterViewModel : IEntityComponentParameterViewModel
  {
    #region Fields and Properties

    private readonly EntityComponentViewModel component;
    public object? cachedValue;

    public MemberInfo MemberInfo { get; }

    public Type ParameterType { get; }

    #endregion

    #region Constructor

    public ParameterViewModel(EntityComponentViewModel component, PropertyInfo propertyInfo)
      : this(component, propertyInfo, propertyInfo.PropertyType)
    {
    }

    public ParameterViewModel(EntityComponentViewModel component, FieldInfo fieldInfo)
      : this(component, fieldInfo, fieldInfo.FieldType)
    {
    }

    private ParameterViewModel(EntityComponentViewModel component, MemberInfo memberInfo, Type parameterType)
    {
      this.component = component;
      this.MemberInfo = memberInfo;
      this.ParameterType = parameterType;
    }

    #endregion

    #region Methods

    public void InvalidateCachedValue()
    {
      var value = this.component.GetValue(this);
      if (this.cachedValue == value)
        return;

      this.cachedValue = value;
      this.OnPropertyChanged();
    }

    private void OnPropertyChanged()
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(this.Name));
    }

    #endregion

    #region IEntityComponentParameterViewModel

    public string Name => this.MemberInfo.Name;

    public object? Value => this.cachedValue;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetValue(object? value)
    {
      if (this.cachedValue == value)
        return;

      this.cachedValue = value;
      this.component.SetValue(this, value);
      this.OnPropertyChanged();
    }

    #endregion
  }

  #endregion
}
