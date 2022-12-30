using Genius.Atom.UI.Forms.Wpf.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnAttachedViewBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var attachedViewType = context.BuildViewColumn?.AttachedViewType;
        if (attachedViewType is null)
        {
            return;
        }

        context.Args.Column = DataGridColumnBuilder
            .ForValuePath(context.Property.Name)
            .RenderAsViewContent(attachedViewType)
            .Build();
    }
}
