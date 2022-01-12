using System.Text.RegularExpressions;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnHeaderNameBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (context.Args.Column.Header is not string headerText)
        {
            return;
        }

        var title = context.GetAttribute<TitleAttribute>()?.Title;

        context.Args.Column.Header = title ?? Regex.Replace(headerText, "[A-Z]", " $0");
    }
}
