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
    extension<TViewModel>(TViewModel viewModel)
        where TViewModel : IViewModel
    {
        /// <summary>
        ///   Returns a disposable object which is invoking a provided handler every time
        ///   when the view model property defined in the <paramref name="propertyAccessor"/>
        ///   parameter has changed in the instance of view model.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyAccessor">An expression which points to the property.</param>
        /// <param name="handler">A handler to An expression which points to the property.</param>
        /// <returns>An observable.</returns>
        public IDisposable WhenChanged<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<TProperty> handler)
        {
            var propName = ExpressionHelpers.GetPropertyName(propertyAccessor);

            return viewModel.WhenChanged(propName, handler);
        }

        /// <summary>
        ///   Returns an observable which is triggered every time when the view model property defined
        ///   in the <paramref name="propertyAccessor"/> parameter has changed in the instance of view model.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyAccessor">An expression which points to the property.</param>
        /// <returns>An observable.</returns>
        public IObservable<TProperty?> WhenChanged<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor)
        {
            string propertyName = ExpressionHelpers.GetPropertyName(propertyAccessor);

            return ViewModelExtensions.WhenChanged<TViewModel, TProperty>(viewModel, propertyName);
        }

        /// <summary>
        ///   Returns an observable which is triggered every time when the view model property defined
        ///   in the <paramref name="propertyName"/> parameter has changed in the instance of view model.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyName">The property name.</param>
        /// <returns>An observable.</returns>
        public IObservable<TProperty?> WhenChanged<TProperty>(string propertyName)
        {
            ViewModelBase viewModelBase = (viewModel as ViewModelBase).NotNull();

            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => viewModel.PropertyChanged += h, h => viewModel.PropertyChanged -= h)
                .Where(x => propertyName.Equals(x.EventArgs.PropertyName, StringComparison.Ordinal))
                .Select(_ => viewModelBase.TryGetPropertyValue(propertyName, out object? value)
                    ? (TProperty?)value
                    : default);
        }

        /// <summary>
        ///   Returns an observable which is triggered every time when any of the view model properties
        ///   defined in the <paramref name="propertyAccessors"/> parameter have changed in the instance
        ///   of view model.
        /// </summary>
        /// <param name="propertyAccessors">Expressions which point to the properties.</param>
        /// <returns>An observable.</returns>
        public IObservable<Unit> WhenAnyChanged(params Expression<Func<TViewModel, object?>>[] propertyAccessors)
        {
            var propNames = propertyAccessors.Select(x => ExpressionHelpers.GetPropertyName(x)).ToArray();

            return viewModel.WhenAnyChanged(propNames);
        }

        /// <summary>
        ///   Returns an observable which is triggered every time when any of the view model properties
        ///   defined in the <paramref name="propertyNames"/> parameter have changed in the instance
        ///   of view model.
        /// </summary>
        /// <param name="propertyNames">The names of the properties.</param>
        /// <returns>An observable.</returns>
        public IObservable<Unit> WhenAnyChanged(string[] propertyNames)
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => viewModel.PropertyChanged += h, h => viewModel.PropertyChanged -= h)
                .Where(x => propertyNames.Length == 0 || propertyNames.Contains(x.EventArgs.PropertyName))
                .Select(_ => Unit.Default);
        }

        /// <summary>
        ///   Returns an observable which is triggered every time when a validation error is added or removed.
        /// </summary>
        /// <returns>An observable.</returns>
        public IObservable<DataErrorsChangedEventArgs> WhenError()
        {
            return Observable.FromEventPattern<EventHandler<DataErrorsChangedEventArgs>, DataErrorsChangedEventArgs>(
                h => viewModel.ErrorsChanged += h, h => viewModel.ErrorsChanged -= h)
                .Select(x => x.EventArgs);
        }
    }

    extension(IViewModel viewModel)
    {
        public IDisposable WhenChanged<TProperty>(string propertyName, Action<TProperty> handler)
        {
            void fn(object? _, PropertyChangedEventArgs args)
            {
                if (args.PropertyName != propertyName)
                    return;

                if (!viewModel.TryGetPropertyValue(propertyName, out var value))
                    return;

                handler((TProperty)value!);
            }

            viewModel.PropertyChanged += fn;

            return new DisposableAction(() => viewModel.PropertyChanged -= fn);
        }

        public IDisposable WhenChanged(string propertyName, Action handler)
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
    }
}
