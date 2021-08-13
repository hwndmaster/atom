using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using Genius.Atom.Infrastructure;

namespace Genius.Atom.UI.Forms.ViewModels
{
    public static class ViewModelExtensions
    {
        public static IDisposable WhenChanged<TViewModel, TProperty>(this TViewModel viewModel, Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<TProperty> handler)
            where TViewModel : IViewModel
        {
            var propName = ExpressionHelpers.GetPropertyName(propertyAccessor);

            return WhenChanged(viewModel, propName, handler);
        }

        public static IDisposable WhenChanged<TProperty>(this IViewModel viewModel, string propertyName, Action<TProperty> handler)
        {
            void fn(object _, PropertyChangedEventArgs args)
            {
                if (args.PropertyName != propertyName)
                    return;

                if (!viewModel.TryGetPropertyValue(propertyName, out var value))
                    return;

                handler((TProperty) value);
            }

            viewModel.PropertyChanged += fn;

            return new DisposableAction(() => viewModel.PropertyChanged -= fn);
        }

        public static IObservable<Unit> WhenAnyChanged<TViewModel>(this TViewModel viewModel,
            params Expression<Func<TViewModel, object>>[] propertyAccessors)
            where TViewModel : IViewModel
        {
            var propNames = propertyAccessors.Select(x => ExpressionHelpers.GetPropertyName(x)).ToArray();

            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => viewModel.PropertyChanged += h, h => viewModel.PropertyChanged -= h)
                .Where(x => propNames.Contains(x.EventArgs.PropertyName))
                .Select(_ => Unit.Default);
        }
    }
}
