using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Wpf;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnTooltipBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (context.BuildColumn.ToolTipPath is not null)
        {
            var style = StylingHelpers.EnsureDefaultCellStyle(context.Args.Column);
            var binding = new Binding(context.BuildColumn.ToolTipPath);
            style.Setters.Add(new Setter(ToolTipService.ToolTipProperty, binding));
        }
        else if (context.BuildColumn.ToolTip is not null)
        {
            var style = StylingHelpers.EnsureDefaultCellStyle(context.Args.Column);
            style.Setters.Add(new Setter(ToolTipService.ToolTipProperty, context.BuildColumn.ToolTip));
        }
    }
}
