using Genius.Atom.UI.Forms.Wpf.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnTextBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (typeof(bool).IsAssignableFrom(context.Property.PropertyType)
            && context.BuildColumn.ValueConverter?.GetType().BaseType == typeof(MarkupBooleanConverterBase<string>))
        {
            context.Args.Column = DataGridColumnBuilder.ForValuePath(context.Property.Name)
                .BasedOnAutoGridColumnContext(context)
                .RenderAsText()
                .Build();
        }
    }
}
