namespace Genius.Atom.UI.Forms.Controls.AutoGrid.ColumnBehaviors;

internal sealed class ColumnConverterBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var binding = context.GetBinding();
        if (binding is null)
        {
            return;
        }

        if (context.BuildColumn.ValueConverter is not null)
        {
            binding.Converter = context.BuildColumn.ValueConverter;
        }
    }
}
