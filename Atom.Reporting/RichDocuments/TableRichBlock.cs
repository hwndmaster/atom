/*
 * References:
 *   https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/Table.cs
 *   https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/TableColumn.cs
 *   https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/TableRow.cs
 */

using System.Drawing;

namespace Genius.Atom.Reporting.RichDocuments;

public sealed class TableRichBlock : RichBlock
{
    public double CellSpacing { get; set; }
    public IList<TableColumnRichBlock> Columns { get; } = new List<TableColumnRichBlock>();
    public IList<TableRowRichBlock> Rows { get; } = new List<TableRowRichBlock>();
}

public sealed class TableColumnRichBlock : RichBlock
{
    public GridLength Width { get; set; }
}

public sealed class TableRowRichBlock : RichBlock
{
    public IList<TableCellRichBlock> Cells { get; } = new List<TableCellRichBlock>();
}

public sealed class TableCellRichBlock : RichBlock
{
    public IList<RichBlock> Blocks { get; } = new List<RichBlock>();
    public int ColumnSpan { get; set; }
    public int RowSpan { get; set; }
}
