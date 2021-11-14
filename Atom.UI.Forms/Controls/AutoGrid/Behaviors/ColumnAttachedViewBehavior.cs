using Genius.Atom.UI.Forms.WpfBuilders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal class ColumnAttachedViewBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var attachedViewAttr = context.GetAttribute<AttachedViewAttribute>();
        if (attachedViewAttr == null)
        {
            return;
        }

        context.Args.Column = DataGridColumnBuilder
            .ForValuePath(context.Property.Name)
            .WithViewContent(attachedViewAttr.AttachedViewType)
            .Build();
    }
}
