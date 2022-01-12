using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Genius.Atom.UI.Forms;

public interface IViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    bool TryGetPropertyValue(string propertyName, out object? value);
}

public abstract class ViewModelBase : IViewModel
{
    protected readonly ConcurrentDictionary<string, object?> _propertyBag = new();
    private readonly Dictionary<string, List<ValidationRule>> _validationRules = new();
    private readonly Dictionary<string, List<string>> _errors = new();
    private bool _suspendDirtySet = false;

    protected ViewModelBase()
    {
        DetectValidationRules();
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        return string.IsNullOrWhiteSpace(propertyName)
            ? _errors.SelectMany(entry => entry.Value)
            : _errors.TryGetValue(propertyName, out List<string>? errors)
                ? errors
                : new List<string>();
    }

    public bool TryGetPropertyValue(string propertyName, out object? value)
    {
        return _propertyBag.TryGetValue(propertyName, out value);
    }

    protected void InitializeProperties(Action action)
    {
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

    protected T GetOrDefault<T>([AllowNull] T defaultValue = default, [CallerMemberName] string? name = null)
    {
        Guard.NotNull(name, nameof(name));

        var result = _propertyBag.GetOrAdd(name, _ => defaultValue);

        return (T) result!;
    }

    protected void RaiseAndSetIfChanged<T>(T value, Action<T, T>? valueChangedHandler = null, [CallerMemberName] string? name = null)
    {
        Guard.NotNull(name, nameof(name));

        var isInitial = !_propertyBag.TryGetValue(name, out object? oldValue);

        if (Equals(oldValue, value))
        {
            if (isInitial)
            {
                // Initial validation
                ValidateProperty(name, value);
            }
            return;
        }

        _propertyBag.AddOrUpdate(name, _ => value, (_, __) => value);
        OnPropertyChanged(name);

        if (!_suspendDirtySet &&
            this is IHasDirtyFlag hasDirtyFlag &&
            name != nameof(IHasDirtyFlag.IsDirty) &&
            (this is not ISelectable || name != nameof(ISelectable.IsSelected)))
        {
            hasDirtyFlag.IsDirty = true;
        }

        ValidateProperty(name, value);

        valueChangedHandler?.Invoke(isInitial ? value : (T)oldValue!, value);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            AddError(propertyName, rule.Validate(value, CultureInfo.CurrentCulture));
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
            errors = new List<string>();
            _errors.Add(propertyName, errors);
        }

        errors.Add(validationResult.ErrorContent.ToString()!);
        OnErrorsChanged(propertyName);
    }

    private void DetectValidationRules()
    {
        var allProperties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in allProperties)
        {
            _validationRules.Add(prop.Name, new List<ValidationRule>());
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
                _validationRules[prop.Name].Add(validationRule);
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public virtual bool HasErrors => _errors.Any();
}
