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

        context.Args.Column = WpfBuilders.DataGridColumnBuilder
            .ForValuePath(context.Property.Name)
            .WithImageSource(iconSource.IconPropertyPath, iconSource.FixedSize, iconSource.HideText)
            .Build();
    }
}
