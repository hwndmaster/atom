using System.Collections.Immutable;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal sealed class AutoGridBuildContext
{
    private readonly Lazy<AutoGridBuildColumnContext[]> _groupByPropertiesLazy;
    private readonly Lazy<AutoGridBuildColumnContext[]> _filterByPropertiesLazy;

    internal AutoGridBuildContext(IEnumerable<AutoGridBuildColumnContext> columns,
        IFactory<object> recordFactory)
    {
        Columns = columns.NotNull().ToImmutableArray();
        RecordFactory = recordFactory.NotNull();

        _groupByPropertiesLazy = new(() => Columns
            .Where(x => x.IsGroupedColumn())
            .ToArray());
        _filterByPropertiesLazy = new(() => Columns
            .OfType<AutoGridBuildTextColumnContext>()
            .Where(x => x.Filterable)
            .ToArray());
    }

    internal static Lazy<AutoGridBuildContext> CreateLazy(DataGrid dataGrid)
    {
        var autoGridBuilder = Properties.GetAutoGridBuilder(dataGrid);
        if (autoGridBuilder is not null)
        {
            return new(() =>
            {
                var buildContext = Properties.GetBuildContext(dataGrid);
                if (buildContext is not null)
                    return buildContext;

                var contextBuilder = autoGridBuilder.Build() as IHasBuildContext
                    ?? throw new InvalidOperationException("A wrong type of context builder has been returned.");
                buildContext = contextBuilder.Build();
                Properties.SetBuildContext(dataGrid, buildContext);
                return buildContext;
            });
        }
        else
        {
            return new(() => {
                var listItemType = Helpers.GetListItemType(dataGrid.ItemsSource);
                return Module.ServiceProvider
                    .GetRequiredService<DefaultAutoGridBuilder>()
                    .ForType(listItemType)
                    .Build();
            });
        }
    }

    public ImmutableArray<AutoGridBuildColumnContext> Columns { get; }

    public IFactory<object> RecordFactory { get; init; }

    public bool EnableVirtualization { get; init; }
    public string? FilterContextScope { get; init; }
    public bool MakeReadOnly { get; init; }

    // Calculatable properties:
    public AutoGridBuildColumnContext[] GroupByProperties => _groupByPropertiesLazy.Value;
    public AutoGridBuildColumnContext[] FilterByProperties => _filterByPropertiesLazy.Value;
}
