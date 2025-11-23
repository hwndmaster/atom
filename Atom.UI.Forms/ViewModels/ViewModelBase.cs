using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Genius.Atom.Infrastructure.Threading;
using Genius.Atom.UI.Forms.Validation;

namespace Genius.Atom.UI.Forms;

public interface IViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    bool TryGetPropertyValue(string propertyName, out object? value);
    void Validate();
}

/// <summary>
///   An abstract class for view models.
/// </summary>
public abstract class ViewModelBase : IViewModel
{
    private readonly Dictionary<string, List<string>> _errors = [];
    private readonly Dictionary<string, List<PropertyValidation>> _validationRules = [];
    private ConcurrentDictionary<string, object?>? _propertyBag;
    private bool _suspendDirtySet;

    protected ViewModelBase(bool determineValidationRulesFromAttributes = false)
    {
        if (determineValidationRulesFromAttributes)
        {
            DetectValidationRules();
        }
    }

    /// <summary>
    ///   Returns the validation errors for a specified property or for the entire entity.
    /// </summary>
    /// <param name="propertyName">
    ///   The name of the property to retrieve validation errors for;
    ///   or <c>null</c> or <c>String.Empty</c>, to retrieve entity-level errors.
    /// </param>
    /// <returns>The validation errors for the property or entity.</returns>
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return _errors.SelectMany(entry => entry.Value);

        return _errors.TryGetValue(propertyName, out List<string>? errors) ? errors : [];
    }

    public bool TryGetPropertyValue(string propertyName, out object? value)
    {
        return GetPropertyBag().TryGetValue(propertyName, out value);
    }

    /// <summary>
    ///   Validates all properties for which the validation rules are available.
    /// </summary>
    /// <remarks>
    ///   Once validated, if there are any validation errors the <see cref="HasErrors"/> will be updated.
    /// </remarks>
    public void Validate()
    {
        foreach (var rule in _validationRules)
        {
            TryGetPropertyValue(rule.Key, out var value);
            ValidateProperty(rule.Key, value);
        }
    }

    /// <summary>
    ///   Assigns a validation rule on a property, which is taken from <see cref="validationRule.PropertyName" />.
    /// </summary>
    /// <param name="validationRule">The validation rule.</param>
    /// <param name="shouldValidatePropertyName">
    ///   (Optional) The name of a boolean property which is used to check whether the validation
    ///   should be applied on this <paramref name="propertyName"/>. If specified, this also adds
    ///   an additional subscription over <paramref name="shouldValidatePropertyName"/> property
    ///   value change to revalidate the <paramref name="propertyName"/>.
    /// </param>
    protected void AddValidationRule<TValidationRule>(TValidationRule validationRule, string? shouldValidatePropertyName = null)
        where TValidationRule : ValidationRule, IPropertyValidationRule
    {
        Guard.NotNull(validationRule);

        AddValidationRule(validationRule.PropertyName, validationRule, shouldValidatePropertyName);
    }

    /// <summary>
    ///   Assigns a validation rule on a property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="validationRule">The validation rule.</param>
    /// <param name="shouldValidatePropertyName">
    ///   (Optional) The name of a boolean property which is used to check whether the validation
    ///   should be applied on this <paramref name="propertyName"/>. If specified, this also adds
    ///   an additional subscription over <paramref name="shouldValidatePropertyName"/> property
    ///   value change to revalidate the <paramref name="propertyName"/>.
    /// </param>
    protected void AddValidationRule(string propertyName, ValidationRule validationRule, string? shouldValidatePropertyName = null)
    {
        if (!_validationRules.TryGetValue(propertyName, out List<PropertyValidation>? validationRules))
        {
            validationRules = [];
            _validationRules.Add(propertyName, validationRules);
        }

        if (shouldValidatePropertyName is not null
            && !validationRules.Exists(x => x.ShouldValidatePropertyName == shouldValidatePropertyName))
        {
            WhenChangedNoDispose(shouldValidatePropertyName, () =>
            {
                TryGetPropertyValue(propertyName, out var value);
                ValidateProperty(propertyName, value);
            });
        }

        validationRules.Add(new PropertyValidation(validationRule, shouldValidatePropertyName));
    }

    /// <summary>
    ///   Assigns a validation rule on a set of properties. This makes the properties bound and when
    ///   any of the properties got changed it provokes other properties to be revalidated.
    /// </summary>
    /// <param name="propertyNames">The property names.</param>
    /// <param name="validationRule">The validation rule.</param>
    protected void AddValidationRule(string[] propertyNames, ValidationRule validationRule)
    {
        Guard.NotNull(propertyNames);

        foreach (var propertyName in propertyNames)
        {
            AddValidationRule(propertyName, validationRule);

            WhenChangedNoDispose(propertyName, () =>
            {
                var linkedProperties = propertyNames.Where(x => !x.Equals(propertyName, StringComparison.Ordinal));
                foreach (var linkedProperty in linkedProperties)
                {
                    TryGetPropertyValue(linkedProperty, out var value);
                    ValidateProperty(linkedProperty, value);
                }
            });
        }
    }

    /// <summary>
    ///   Returns true if the property has validation errors. Otherwise it returns false.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>True is property has errors, otherwise false.</returns>
    protected bool PropertyHasErrors(string propertyName)
        => _errors.TryGetValue(propertyName, out List<string>? propertyErrors) && propertyErrors.Any();

    protected void InitializeProperties(Action action)
    {
        Guard.NotNull(action);

        _suspendDirtySet = true;
        try
        {
            action();

            if (this is IHasDirtyFlag hasDirtyFlag)
            {
                hasDirtyFlag.IsDirty = false;
            }
        }
        finally
        {
            _suspendDirtySet = false;
        }
    }

    /// <summary>
    ///   Returns a property value if it exists in the property bag, otherwise it returns a default value.
    /// </summary>
    /// <typeparam name="TValue">The type of the property.</typeparam>
    /// <param name="defaultValue">The default property value.</param>
    /// <param name="propertyName">The property name. If the method is called within the property getter it hasn't to be specified.</param>
    /// <returns>The property value from the property bag.</returns>
    [return: NotNullIfNotNull("defaultValue")]
    protected TValue GetOrDefault<TValue>([AllowNull] TValue defaultValue = default, [CallerMemberName] string? propertyName = null)
    {
        Guard.NotNull(propertyName);

        var result = GetPropertyBag().GetOrAdd(propertyName, _ => defaultValue);

        return (TValue)result!;
    }

    /// <summary>
    ///   Returns a property value if it exists in the property bag, otherwise it returns a default value.
    /// </summary>
    /// <typeparam name="TValue">The type of the property.</typeparam>
    /// <param name="defaultValue">A function which returns a default property value, when requested.</param>
    /// <param name="propertyName">The property name. If the method is called within the property getter it hasn't to be specified.</param>
    /// <returns>The property value from the property bag.</returns>
    [return: NotNullIfNotNull("defaultValue")]
    protected TValue GetOrDefault<TValue>([AllowNull] Func<TValue> defaultValue, [CallerMemberName] string? propertyName = null)
    {
        Guard.NotNull(defaultValue);
        Guard.NotNull(propertyName);

        var result = GetPropertyBag().GetOrAdd(propertyName, _ => defaultValue());

        return (TValue)result!;
    }

    /// <summary>
    ///   Sets the value to the specified property and raises an event if value has been changed.
    /// </summary>
    /// <typeparam name="TValue">The type of the property.</typeparam>
    /// <param name="propertyValue">The property value.</param>
    /// <param name="propertyName">The property name. If the method is called within the property getter it hasn't to be specified.</param>
    protected void RaiseAndSetIfChanged<TValue>(TValue propertyValue, Action<TValue, TValue>? valueChangedHandler = null, [CallerMemberName] string? propertyName = null)
    {
        Guard.NotNull(propertyName);

        var isInitial = !GetPropertyBag().TryGetValue(propertyName, out object? oldValue);

        if (Equals(oldValue, propertyValue))
        {
            if (isInitial)
            {
                // Initial validation
                ValidateProperty(propertyName, propertyValue);
            }
            return;
        }

        GetPropertyBag().AddOrUpdate(propertyName, _ => propertyValue, (_, __) => propertyValue);
        ValidateProperty(propertyName, propertyValue);
        valueChangedHandler?.Invoke(isInitial ? propertyValue : (TValue)oldValue!, propertyValue);

        OnPropertyChanged(propertyName);

        if (!_suspendDirtySet &&
            this is IHasDirtyFlag hasDirtyFlag
            && propertyName != nameof(IHasDirtyFlag.IsDirty)
            && (this is not ISelectable || propertyName != nameof(ISelectable.IsSelected))
            && (this is not IEditable || propertyName != nameof(IEditable.IsEditing)))
        {
            hasDirtyFlag.IsDirty = true;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void WhenChangedNoDispose(string propertyName, Action handler)
    {
        // It is safe to handle a property changed event within the view model without disposing the subscription.
#pragma warning disable CA2000 // Dispose objects before losing scope
        this.WhenChanged(propertyName, handler);
#pragma warning restore CA2000 // Dispose objects before losing scope
    }

    protected void WhenAnyChangedNoDispose(string[] propertyNames, Action handler)
    {
        // It is safe to handle a property changed event within the view model without disposing the subscription.
        this.WhenAnyChanged(propertyNames)
            .Subscribe(x => handler());
    }

    protected void WhenAnyChangedNoDispose(string[] propertyNames, Func<Task> handler)
    {
        // It is safe to handle a property changed event within the view model without disposing the subscription.

        this.WhenAnyChanged(propertyNames)
            .Subscribe(_ => {
                ViewModelStatic.JoinableTaskFactory.Run(async () => await handler());
            });
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ValidateProperty(string propertyName, object? value)
    {
        // Clear previous errors of the current property to be validated
        _errors.Remove(propertyName);
        OnErrorsChanged(propertyName);

        if (!_validationRules.TryGetValue(propertyName, out var rules))
        {
            return;
        }

        foreach (var rule in rules)
        {
            if (rule.ShouldValidatePropertyName is not null
                && TryGetPropertyValue(rule.ShouldValidatePropertyName, out var shouldValidate)
                && shouldValidate is bool shouldValidateAsBoolean
                && !shouldValidateAsBoolean)
            {
                continue;
            }

            AddError(propertyName, rule.ValidationRule.Validate(value, CultureInfo.CurrentCulture));
        }
    }

    private void AddError(string propertyName, ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            return;
        }

        if (!_errors.TryGetValue(propertyName, out var errors))
        {
            errors = [];
            _errors.Add(propertyName, errors);
        }

        var errorMessage = validationResult.ErrorContent.ToString()!;
        if (!errors.Contains(errorMessage))
        {
            errors.Add(errorMessage);
            OnErrorsChanged(propertyName);
        }
    }

    private void DetectValidationRules()
    {
        var allProperties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in allProperties)
        {
            _validationRules.Add(prop.Name, []);
            foreach (var attr in prop.GetCustomAttributes<ValidationRuleAttribute>())
            {
                var hasParams = attr.Parameters?.Any() == true;
                var parameters = hasParams
                    ? new object[] { this }.Concat(attr.Parameters!).ToArray()
                    : null;
                var validationRule = (hasParams
                    ? Activator.CreateInstance(attr.ValidationRuleType, parameters)
                    : Activator.CreateInstance(attr.ValidationRuleType)) as ValidationRule;
                if (validationRule is null)
                {
                    throw new InvalidOperationException($"Validation rule {attr.ValidationRuleType.FullName} cannot be created.");
                }
                _validationRules[prop.Name].Add(new PropertyValidation(validationRule, null));
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ConcurrentDictionary<string, object?> GetPropertyBag()
    {
        return _propertyBag ??= new();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///   Occurs when the validation errors have changed for a property or for the entire entity.
    /// </summary>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    ///   Gets a value indicating whether the entity has validation errors.
    /// </summary>
    public virtual bool HasErrors => _errors.Any();


    private readonly record struct PropertyValidation(ValidationRule ValidationRule, string? ShouldValidatePropertyName);
}
