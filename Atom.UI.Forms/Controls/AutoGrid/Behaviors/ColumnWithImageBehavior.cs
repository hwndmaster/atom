namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal class ColumnWithImageBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var iconAttr = context.GetAttribute<IconSourceAttribute>();
        if (iconAttr == null)
        {
            return;
        }

        context.Args.Column = WpfBuilders.DataGridColumnBuilder
            .ForValuePath(context.Property.Name)
            .WithImageSource(iconAttr.IconPropertyPath, iconAttr.FixedSize, iconAttr.HideText)
            .Build();
    }
}
