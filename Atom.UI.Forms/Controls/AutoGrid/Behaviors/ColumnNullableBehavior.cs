namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnNullableBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (Nullable.GetUnderlyingType(context.Property.PropertyType) is not null)
        {
            context.GetBinding().NotNull().TargetNullValue = string.Empty;
        }
    }
}
