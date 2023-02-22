using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilder
{
    /// <summary>
    ///   Creates a new DataGrid context.
    /// </summary>
    AutoGridBuildContext Build();
}

public interface IAutoGridContextBuilder<TViewModel> : IAutoGridContextBuilder
    where TViewModel : class, IViewModel
{
    /// <summary>
    ///   Enables row and groups virtualization for better performance.
    ///   NOTE: May cause side effects, such as binding issues reported to the console.
    /// </summary>
    IAutoGridContextBuilder<TViewModel> EnableVirtualization();

    /// <summary>
    ///   Makes the DataGrid completely readonly.
    /// </summary>
    IAutoGridContextBuilder<TViewModel> MakeReadOnly();

    /// <summary>
    ///   Defines the columns to show.
    /// </summary>
    /// <param name="columnsBuilderAction"></param>
    IAutoGridContextBuilder<TViewModel> WithColumns(Action<IAutoGridContextBuilderColumns<TViewModel>> columnsBuilderAction);

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
    IAutoGridContextBuilder<TViewModel> WithFilterContextScope(string scopeName);

    /// <summary>
    ///   Defines the view model factory type which can be injected and used by the DataGrid when creating a new row.
    ///   NOTE: If the type isn't registered in the DI container, a parameterless constructor will be used then.
    /// </summary>
    /// <typeparam name="TFactory">The factory type.</typeparam>
    IAutoGridContextBuilder<TViewModel> WithRecordFactory<TFactory>()
        where TFactory : IFactory<TViewModel>;
}

internal sealed class AutoGridContextBuilder<TViewModel> : IAutoGridContextBuilder<TViewModel>
    where TViewModel : class, IViewModel
{
    private readonly AutoGridContextBuilderColumns<TViewModel> _columnsBuilder;
    private readonly List<AutoGridBuildColumnContext> _columns = new();
    private bool _enableVirtualization = false;
    private bool _makeReadOnly = false;
    private string? _filterContextScope = null;
    private IFactory<TViewModel>? _recordFactory;

    public AutoGridContextBuilder(AutoGridContextBuilderColumns<TViewModel> columnsBuilder)
    {
        _columnsBuilder = columnsBuilder.NotNull();
    }

    public IAutoGridContextBuilder<TViewModel> EnableVirtualization()
    {
        _enableVirtualization = true;

        return this;
    }

    public IAutoGridContextBuilder<TViewModel> MakeReadOnly()
    {
        _makeReadOnly = true;

        return this;
    }

    public IAutoGridContextBuilder<TViewModel> WithColumns(Action<IAutoGridContextBuilderColumns<TViewModel>> columnsBuilderAction)
    {
        columnsBuilderAction(_columnsBuilder);

        _columns.AddRange(_columnsBuilder.Build());

        return this;
    }

    public IAutoGridContextBuilder<TViewModel> WithFilterContextScope(string scopeName)
    {
        _filterContextScope = scopeName;

        return this;
    }

    public IAutoGridContextBuilder<TViewModel> WithRecordFactory<TFactory>()
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
        };
    }
}
