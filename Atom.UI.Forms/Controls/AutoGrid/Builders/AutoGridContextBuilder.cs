using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilder
{
}

public interface IAutoGridContextBuilder<TViewModel, TParentViewModel> : IAutoGridContextBuilder
    where TViewModel : class, IViewModel
    where TParentViewModel : IViewModel
{
    /// <summary>
    ///   Enables row and groups virtualization for better performance.
    ///   NOTE: May cause side effects, such as binding issues reported to the console.
    /// </summary>
    IAutoGridContextBuilder<TViewModel, TParentViewModel> EnableVirtualization();

    /// <summary>
    ///   Makes the DataGrid completely readonly.
    /// </summary>
    IAutoGridContextBuilder<TViewModel, TParentViewModel> MakeReadOnly();

    /// <summary>
    ///   Defines the columns to show.
    /// </summary>
    /// <param name="columnsBuilderAction"></param>
    IAutoGridContextBuilder<TViewModel, TParentViewModel> WithColumns(Action<IAutoGridContextBuilderColumns<TViewModel, TParentViewModel>> columnsBuilderAction);

    /// <summary>
    ///   Specifies the scope name for the filter context. The parent view model should contain a property marked with a <see cref="FilterContextAttribute"/>
    ///   attribute with the ScopeName set to <paramref name="scopeName"/>.
    /// </summary>
    /// <param name="scopeName">The scope name.</param>
    /// <example>
    ///   In the parent view model:
    ///   <code>
    ///   [FilterContext(Scope = "SpecificScopeName")]
    ///   public string Filter
    ///   {
    ///     get => GetOrDefault<string>();
    ///     set => RaiseAndSetIfChanged(value);
    ///   }
    ///   </code>
    ///
    ///   In the AutoGrid builder:
    ///   <code>
    ///   _contextBuilderFactory.Create()
    ///      .WithColumns(...)
    ///      .WithFilterContextScope("SpecificScopeName")
    ///      .Build()
    ///   </code>
    /// </example>
    IAutoGridContextBuilder<TViewModel, TParentViewModel> WithFilterContextScope(string scopeName);

    /// <summary>
    ///   Lets the datagrid to optionally enable grouping by switching <paramref name="switchPropertyAccessor"/>
    ///   to show a value of <paramref name="valuePropertyAccessor"/>.
    /// </summary>
    /// <param name="switchPropertyAccessor">Points to a property which indicates whether to enable grouping or not.</param>
    /// <param name="valuePropertyAccessor">Points to a property which contains a structured value shown in the group header.</param>
    IAutoGridContextBuilder<TViewModel, TParentViewModel> WithOptionalGrouping(
        Expression<Func<TParentViewModel, bool>> switchPropertyAccessor,
        Expression<Func<TViewModel, IGroupableViewModel?>> valuePropertyAccessor);

    /// <summary>
    ///   Defines the view model factory type which can be injected and used by the DataGrid when creating a new row.
    ///   NOTE: If the type isn't registered in the DI container, a parameterless constructor will be used then.
    /// </summary>
    /// <typeparam name="TFactory">The factory type.</typeparam>
    IAutoGridContextBuilder<TViewModel, TParentViewModel> WithRecordFactory<TFactory>()
        where TFactory : IFactory<TViewModel>;
}

internal sealed class AutoGridContextBuilder<TViewModel, TParentViewModel>
    : IAutoGridContextBuilder<TViewModel, TParentViewModel>,
        IHasBuildContext
    where TViewModel : class, IViewModel
    where TParentViewModel : IViewModel
{
    private readonly AutoGridContextBuilderColumns<TViewModel, TParentViewModel> _columnsBuilder;
    private readonly List<AutoGridBuildColumnContext> _columns = [];
    private bool _enableVirtualization;
    private bool _makeReadOnly;
    private string? _filterContextScope;

    /// <summary>
    ///   A property name in <see cref="TParentViewModel"/>.
    /// </summary>
    private string? _optionalGroupingSwitchProperty;

    /// <summary>
    ///   A property name in <see cref="TViewModel"/>.
    /// </summary>
    private string? _optionalGroupingValueProperty;

    private IFactory<TViewModel>? _recordFactory;

    public AutoGridContextBuilder(AutoGridContextBuilderColumns<TViewModel, TParentViewModel> columnsBuilder)
    {
        _columnsBuilder = columnsBuilder.NotNull();
    }

    public IAutoGridContextBuilder<TViewModel, TParentViewModel> EnableVirtualization()
    {
        _enableVirtualization = true;

        return this;
    }

    public IAutoGridContextBuilder<TViewModel, TParentViewModel> MakeReadOnly()
    {
        _makeReadOnly = true;

        return this;
    }

    public IAutoGridContextBuilder<TViewModel, TParentViewModel> WithColumns(Action<IAutoGridContextBuilderColumns<TViewModel, TParentViewModel>> columnsBuilderAction)
    {
        columnsBuilderAction(_columnsBuilder);

        _columns.AddRange(_columnsBuilder.Build());

        return this;
    }

    public IAutoGridContextBuilder<TViewModel, TParentViewModel> WithFilterContextScope(string scopeName)
    {
        _filterContextScope = scopeName;

        return this;
    }

    public IAutoGridContextBuilder<TViewModel, TParentViewModel> WithOptionalGrouping(
        Expression<Func<TParentViewModel, bool>> switchPropertyAccessor,
        Expression<Func<TViewModel, IGroupableViewModel?>> valuePropertyAccessor)
    {
        _optionalGroupingSwitchProperty = ExpressionHelpers.GetPropertyName(switchPropertyAccessor);
        _optionalGroupingValueProperty = ExpressionHelpers.GetPropertyName(valuePropertyAccessor);
        return this;
    }

    public IAutoGridContextBuilder<TViewModel, TParentViewModel> WithRecordFactory<TFactory>()
        where TFactory : IFactory<TViewModel>
    {
        _recordFactory = Module.ServiceProvider.GetService<TFactory>().NotNull();
        return this;
    }

    public AutoGridBuildContext Build()
    {
        _recordFactory ??= new ServiceFactory<TViewModel>();

        var recordFactoryProxy = new DefaultFactory<object>(() => _recordFactory.Create());

        return new AutoGridBuildContext(_columns, recordFactoryProxy)
        {
            EnableVirtualization = _enableVirtualization,
            FilterContextScope = _filterContextScope,
            MakeReadOnly = _makeReadOnly,
            OptionalGroupingSwitchProperty = _optionalGroupingSwitchProperty,
            OptionalGroupingValueProperty = _optionalGroupingValueProperty
        };
    }
}
