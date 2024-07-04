namespace Genius.Atom.UI.Forms.Controls.AutoGrid.ColumnBehaviors;

internal sealed class ColumnNullableBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (Nullable.GetUnderlyingType(context.Property.PropertyType) is not null)
        {
            var binding = context.GetBinding();
            if (binding is not null)
            {
                binding.TargetNullValue = string.Empty;
            }
        }
    }
}
