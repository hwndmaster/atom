using System.ComponentModel;
using System.Linq.Expressions;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderColumns<TViewModel, TParentViewModel>
    where TViewModel : class, IViewModel
    where TParentViewModel : IViewModel
{
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddAll();
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddComboBox<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel>>? options = null);
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddComboBox(string propertyName, Action<IAutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel>>? options = null);
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddCommand<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel>>? options = null)
        where TProperty : IActionCommand;
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddCommand(string propertyName, Action<IAutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel>>? options = null);
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddDynamic(
        Expression<Func<TParentViewModel, DynamicColumnsViewModel?>> columnsPropertyAccessor,
        Expression<Func<TViewModel, DynamicColumnEntriesViewModel?>> entriesPropertyAccessor,
        Action<IAutoGridContextBuilderDynamicColumn<TViewModel, TParentViewModel>>? options = null);
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddDynamic(string columnsPropertyName, string entriesPropertyName, Action<IAutoGridContextBuilderDynamicColumn<TViewModel, TParentViewModel>>? options = null);
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddText<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel>>? options = null);
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddText(string propertyName, Action<IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel>>? options = null);
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddToggleButton(Expression<Func<TViewModel, bool>> propertyAccessor, Action<IAutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel>>? options = null);
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddToggleButton(string propertyName, Action<IAutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel>>? options = null);
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddView<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderViewColumn<TViewModel, TParentViewModel>>? options = null);
    IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddView(string propertyName, Action<IAutoGridContextBuilderViewColumn<TViewModel, TParentViewModel>>? options = null);
}

internal sealed class AutoGridContextBuilderColumns<TViewModel, TParentViewModel> : IAutoGridContextBuilderColumns<TViewModel, TParentViewModel>
    where TViewModel : class, IViewModel
    where TParentViewModel : IViewModel
{
    private readonly List<IAutoGridContextBuilderColumn> _columnBuilders = new();
    private readonly PropertyDescriptorCollection _propertyDescriptors;

    public AutoGridContextBuilderColumns()
    {
        _propertyDescriptors = TypeDescriptor.GetProperties(typeof(TViewModel));
    }

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddAll()
    {
        _columnBuilders.AddRange(
            _propertyDescriptors
                .Cast<PropertyDescriptor>()
                .Where(pd =>
                    !AutoGridBuilderHelpers.IsIgnorableProperty(pd.Name)
                    && !pd.Attributes.OfType<BrowsableAttribute>().Any(x => !x.Browsable))
                .Select(pd => AutoGridBuilderHelpers.CreateContextBuilderColumn<TViewModel, TParentViewModel>(pd))
        );

        return this;
    }

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddComboBox<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel>>? options = null)
        => AddComboBox(ExpressionHelpers.GetPropertyName(propertyAccessor), options);

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddComboBox(string propertyName, Action<IAutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel>>? options = null)
        => AddColumnInternal(pd => new AutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel>(pd), propertyName, options);

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddCommand<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel>>? options = null)
        where TProperty : IActionCommand
        => AddCommand(ExpressionHelpers.GetPropertyName(propertyAccessor), options);

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddCommand(string propertyName, Action<IAutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel>>? options = null)
        => AddColumnInternal(pd => new AutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel>(pd), propertyName, options);

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddDynamic(
        Expression<Func<TParentViewModel, DynamicColumnsViewModel?>> columnsPropertyAccessor,
        Expression<Func<TViewModel, DynamicColumnEntriesViewModel?>> entriesPropertyAccessor,
        Action<IAutoGridContextBuilderDynamicColumn<TViewModel, TParentViewModel>>? options = null)
        => AddDynamic(ExpressionHelpers.GetPropertyName(columnsPropertyAccessor), ExpressionHelpers.GetPropertyName(entriesPropertyAccessor), options);

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddDynamic(string columnsPropertyName, string entriesPropertyName, Action<IAutoGridContextBuilderDynamicColumn<TViewModel, TParentViewModel>>? options = null)
        => AddColumnInternal(pd => new AutoGridContextBuilderDynamicColumn<TViewModel, TParentViewModel>(pd, columnsPropertyName), entriesPropertyName, options);

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddText<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel>>? options = null)
        => AddText(ExpressionHelpers.GetPropertyName(propertyAccessor), options);

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddText(string propertyName, Action<IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel>>? options = null)
        => AddColumnInternal(pd => new AutoGridContextBuilderTextColumn<TViewModel, TParentViewModel>(pd), propertyName, options);

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddToggleButton(Expression<Func<TViewModel, bool>> propertyAccessor, Action<IAutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel>>? options = null)
        => AddToggleButton(ExpressionHelpers.GetPropertyName(propertyAccessor), options);

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddToggleButton(string propertyName, Action<IAutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel>>? options = null)
        => AddColumnInternal(pd => new AutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel>(pd), propertyName, options);

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddView<TProperty>(Expression<Func<TViewModel, TProperty>> propertyAccessor, Action<IAutoGridContextBuilderViewColumn<TViewModel, TParentViewModel>>? options = null)
        => AddView(ExpressionHelpers.GetPropertyName(propertyAccessor), options);

    public IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddView(string propertyName, Action<IAutoGridContextBuilderViewColumn<TViewModel, TParentViewModel>>? options = null)
        => AddColumnInternal(pd => new AutoGridContextBuilderViewColumn<TViewModel, TParentViewModel>(pd), propertyName, options);

    internal AutoGridBuildColumnContext[] Build()
    {
        var index = 0;
        return _columnBuilders
            .Cast<IHasBuildColumnContext>()
            .Select((x) =>
            {
                var columnContext = x.BuildInternal();
                if (columnContext is not AutoGridBuildTextColumnContext textColumn
                    || !textColumn.IsGrouped)
                {
                    columnContext.DisplayIndex = index++;
                }
                return columnContext;
            })
            .ToArray();
    }

    private IAutoGridContextBuilderColumns<TViewModel, TParentViewModel> AddColumnInternal<T>(
        Func<PropertyDescriptor, T> createFunc, string propertyName, Action<T>? options = null, bool singleInstance = false)
        where T : IAutoGridContextBuilderColumn
    {
        if (singleInstance && _columnBuilders.Any(x => x is T))
        {
            throw new InvalidOperationException($"There could be only one column registered of type {typeof(T).Name}");
        }

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
