using System.Collections.Immutable;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public sealed class AutoGridBuildContext
{
    internal AutoGridBuildContext(IEnumerable<AutoGridBuildColumnContext> columns,
        IFactory<object> recordFactory)
    {
        Columns = columns.NotNull().ToImmutableArray();
        RecordFactory = recordFactory.NotNull();
    }

    internal static Lazy<AutoGridBuildContext> CreateLazy(DataGrid dataGrid)
    {
        var autoGridBuilder = Properties.GetAutoGridBuilder(dataGrid);
        if (autoGridBuilder is not null)
        {
            return new(() => autoGridBuilder.Build());
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
}
