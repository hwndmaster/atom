using Genius.Atom.UI.Forms.Wpf.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnWithImageBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var iconSource = context.BuildTextColumn?.IconSource;
        if (iconSource is null)
        {
            return;
        }

        context.Args.Column = DataGridColumnBuilder
            .ForValuePath(context.Property.Name)
            .RenderAsTextWithImage(iconSource.IconPropertyPath)
            .WithImageSize(iconSource.FixedSize)
            .WithTextHidden(iconSource.HideText)
            .Build();
    }
}
