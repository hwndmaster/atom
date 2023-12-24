using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;

namespace Genius.Atom.UI.Forms;

/// <summary>
///   This static class contains helper methods over <seealso cref="IViewModel"/> interface
///   to facilitate communication with instances of that interface.
/// </summary>
public static class ViewModelExtensions
{
    /// <summary>
    ///   Returns a disposable object which is invoking a provided handler every time
    ///   when the view model property defined in the <paramref name="propertyAccessor"/>
    ///   parameter has changed in the instance of <paramref name="viewModel"/>.
    /// </summary>
    /// <typeparam name="TViewModel">The concrete type of the view model.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="viewModel">The view model.</param>
    /// <param name="propertyAccessor">An expression which points to the property.</param>
    /// <param name="handler">A handler to An expression which points to the property.</param>
    /// <returns>An observable.</returns>
    public static IDisposable WhenChanged<TViewModel, TProperty>(this TViewModel viewModel,
        Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<TProperty> handler)
        where TViewModel : IViewModel
    {
        var propName = ExpressionHelpers.GetPropertyName(propertyAccessor);

        return WhenChanged(viewModel, propName, handler);
    }

    /// <summary>
    ///   Returns an observable which is triggered every time when the view model property defined
    ///   in the <paramref name="propertyAccessor"/> parameter has changed in the instance of <paramref name="viewModel"/>.
    /// </summary>
    /// <typeparam name="TViewModel">The concrete type of the view model.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="viewModel">The view model.</param>
    /// <param name="propertyAccessor">An expression which points to the property.</param>
    /// <returns>An observable.</returns>
    public static IObservable<TProperty?> WhenChanged<TViewModel, TProperty>(this TViewModel viewModel,
        Expression<Func<TViewModel, TProperty>> propertyAccessor)
        where TViewModel : IViewModel
    {
        string propertyName = ExpressionHelpers.GetPropertyName(propertyAccessor);

        return WhenChanged<TViewModel, TProperty>(viewModel, propertyName);
    }

    /// <summary>
    ///   Returns an observable which is triggered every time when the view model property defined
    ///   in the <paramref name="propertyAccessor"/> parameter has changed in the instance of <paramref name="viewModel"/>.
    /// </summary>
    /// <typeparam name="TViewModel">The concrete type of the view model.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="viewModel">The view model.</param>
    /// <param name="propertyAccessor">An expression which points to the property.</param>
    /// <returns>An observable.</returns>
    public static IObservable<TProperty?> WhenChanged<TViewModel, TProperty>(this TViewModel viewModel,
        string propertyName)
        where TViewModel : IViewModel
    {
        ViewModelBase viewModelBase = (viewModel as ViewModelBase).NotNull();

        return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => viewModel.PropertyChanged += h, h => viewModel.PropertyChanged -= h)
            .Where(x => propertyName.Equals(x.EventArgs.PropertyName, StringComparison.Ordinal))
            .Select(_ => viewModelBase.TryGetPropertyValue(propertyName, out object? value)
                ? (TProperty?)value
                : default);
    }

    public static IDisposable WhenChanged<TProperty>(this IViewModel viewModel, string propertyName, Action<TProperty> handler)
    {
        void fn(object? _, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != propertyName)
                return;

            if (!viewModel.TryGetPropertyValue(propertyName, out var value))
                return;

            handler((TProperty) value!);
        }

        viewModel.PropertyChanged += fn;

        return new DisposableAction(() => viewModel.PropertyChanged -= fn);
    }

    public static IDisposable WhenChanged(this IViewModel viewModel, string propertyName, Action handler)
    {
        void fn(object? _, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != propertyName)
                return;

            handler();
        }

        viewModel.PropertyChanged += fn;

        return new DisposableAction(() => viewModel.PropertyChanged -= fn);
    }

    /// <summary>
    ///   Returns an observable which is triggered every time when any of the view model properties
    ///   defined in the <paramref name="propertyAccessors"/> parameter have changed in the instance
    ///   of <paramref name="viewModel"/>.
    /// </summary>
    /// <typeparam name="TViewModel">The concrete type of the view model.</typeparam>
    /// <param name="viewModel">The view model.</param>
    /// <param name="propertyAccessors">Expressions which point to the properties.</param>
    /// <returns>An observable.</returns>
    public static IObservable<Unit> WhenAnyChanged<TViewModel>(this TViewModel viewModel,
        params Expression<Func<TViewModel, object?>>[] propertyAccessors)
        where TViewModel : IViewModel
    {
        var propNames = propertyAccessors.Select(x => ExpressionHelpers.GetPropertyName(x)).ToArray();

        return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => viewModel.PropertyChanged += h, h => viewModel.PropertyChanged -= h)
            .Where(x => propNames.Length == 0 || propNames.Contains(x.EventArgs.PropertyName))
            .Select(_ => Unit.Default);
    }

    /// <summary>
    ///   Returns an observable which is triggered every time when a validation error is added or removed.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <typeparam name="TViewModel">The concrete type of the view model.</typeparam>
    /// <returns>An observable.</returns>
    public static IObservable<DataErrorsChangedEventArgs> WhenError<TViewModel>(this TViewModel viewModel)
        where TViewModel : IViewModel
    {
        return Observable.FromEventPattern<EventHandler<DataErrorsChangedEventArgs>, DataErrorsChangedEventArgs>(
            h => viewModel.ErrorsChanged += h, h => viewModel.ErrorsChanged -= h)
            .Select(x => x.EventArgs);
    }
}
