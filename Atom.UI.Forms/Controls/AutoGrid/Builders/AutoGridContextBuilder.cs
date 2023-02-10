using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilder
{
    AutoGridBuildContext Build();
}

public interface IAutoGridContextBuilder<TViewModel> : IAutoGridContextBuilder
    where TViewModel : class, IViewModel
{
    IAutoGridContextBuilder<TViewModel> EnableVirtualization();
    IAutoGridContextBuilder<TViewModel> MakeReadOnly();
    IAutoGridContextBuilder<TViewModel> WithColumns(Action<IAutoGridContextBuilderColumns<TViewModel>> columnsBuilderAction);
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
            MakeReadOnly = _makeReadOnly
        };
    }
}
