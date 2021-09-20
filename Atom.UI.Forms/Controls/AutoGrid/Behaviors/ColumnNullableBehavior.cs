using System;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors
{
    internal class ColumnNullableBehavior : IAutoGridColumnBehavior
    {
        public void Attach(AutoGridColumnContext context)
        {
            if (Nullable.GetUnderlyingType(context.Property.PropertyType) != null)
            {
                context.GetBinding().TargetNullValue = string.Empty;
            }
        }
    }
}
