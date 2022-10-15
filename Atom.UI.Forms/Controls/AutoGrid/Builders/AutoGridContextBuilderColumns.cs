using System.ComponentModel;
using System.Linq.Expressions;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderColumns<TViewModel>
        where TViewModel : class, IViewModel
{
    IAutoGridContextBuilderColumns<TViewModel> AddText<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderTextColumn>? options = null);
    IAutoGridContextBuilderColumns<TViewModel> AddText(string propertyName, Action<IAutoGridContextBuilderTextColumn>? options = null);
    IAutoGridContextBuilderColumns<TViewModel> AddCommand<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderCommandColumn>? options = null);
    IAutoGridContextBuilderColumns<TViewModel> AddCommand(string propertyName, Action<IAutoGridContextBuilderCommandColumn>? options = null);
    IAutoGridContextBuilderColumns<TViewModel> AddView<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderViewColumn>? options = null);
    IAutoGridContextBuilderColumns<TViewModel> AddView(string propertyName, Action<IAutoGridContextBuilderViewColumn>? options = null);
    IAutoGridContextBuilderColumns<TViewModel> AddAll();
}

internal sealed class AutoGridContextBuilderColumns<TViewModel> : IAutoGridContextBuilderColumns<TViewModel>
        where TViewModel : class, IViewModel
{
    private readonly List<IAutoGridContextBuilderColumn> _columnBuilders = new();
    private readonly PropertyDescriptorCollection _propertyDescriptors;

    public AutoGridContextBuilderColumns()
    {
        _propertyDescriptors = TypeDescriptor.GetProperties(typeof(TViewModel));
    }

    public IAutoGridContextBuilderColumns<TViewModel> AddText<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderTextColumn>? options = null)
        => AddText(ExpressionHelpers.GetPropertyName(propertyAccessor), options);

    public IAutoGridContextBuilderColumns<TViewModel> AddText(string propertyName, Action<IAutoGridContextBuilderTextColumn>? options = null)
        => AddColumnInternal(pd => new AutoGridContextBuilderTextColumn(pd), propertyName, options);

    public IAutoGridContextBuilderColumns<TViewModel> AddComboBox<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderComboBoxColumn>? options = null)
        => AddComboBox(ExpressionHelpers.GetPropertyName(propertyAccessor), options);

    public IAutoGridContextBuilderColumns<TViewModel> AddComboBox(string propertyName, Action<IAutoGridContextBuilderComboBoxColumn>? options = null)
        => AddColumnInternal(pd => new AutoGridContextBuilderComboBoxColumn(pd), propertyName, options);

    public IAutoGridContextBuilderColumns<TViewModel> AddCommand<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderCommandColumn>? options = null)
        => AddCommand(ExpressionHelpers.GetPropertyName(propertyAccessor), options);

    public IAutoGridContextBuilderColumns<TViewModel> AddCommand(string propertyName, Action<IAutoGridContextBuilderCommandColumn>? options = null)
        => AddColumnInternal(pd => new AutoGridContextBuilderCommandColumn(pd), propertyName, options);

    public IAutoGridContextBuilderColumns<TViewModel> AddView<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderViewColumn>? options = null)
        => AddView(ExpressionHelpers.GetPropertyName(propertyAccessor), options);

    public IAutoGridContextBuilderColumns<TViewModel> AddView(string propertyName, Action<IAutoGridContextBuilderViewColumn>? options = null)
        => AddColumnInternal(pd => new AutoGridContextBuilderViewColumn(pd), propertyName, options);

    public IAutoGridContextBuilderColumns<TViewModel> AddAll()
    {
        _columnBuilders.AddRange(
            _propertyDescriptors.Cast<PropertyDescriptor>()
                .Select(pd => AutoGridBuilderHelpers.CreateContextBuilderColumn(pd))
        );

        return this;
    }

    internal AutoGridBuildColumnContext[] Build()
    {
        return _columnBuilders
            .Cast<IHasBuildColumnContext>()
            .Select(x => x.Build())
            .ToArray();
    }

    private IAutoGridContextBuilderColumns<TViewModel> AddColumnInternal<T>(Func<PropertyDescriptor, T> createFunc, string propertyName, Action<T>? options = null)
        where T : IAutoGridContextBuilderColumn
    {
        var propertyDescriptor = _propertyDescriptors.Find(propertyName, false).NotNull();

        var columnBuilder = createFunc(propertyDescriptor);
        if (options is not null)
        {
            options(columnBuilder);
        }

        _columnBuilders.Add(columnBuilder);

        return this;
    }
}
