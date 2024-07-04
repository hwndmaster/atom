using System.Windows.Data;
using Genius.Atom.UI.Forms.Wpf.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.ColumnBehaviors;

internal sealed class ColumnTextBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var buildTextColumn = context.BuildTextColumn;

        if (buildTextColumn is null)
        {
            return;
        }

        var propertyTypeClean = Nullable.GetUnderlyingType(context.Property.PropertyType)
            ?? context.Property.PropertyType;

        if (
            // Case 1: Convertible with valueconverter
            (typeof(bool).IsAssignableFrom(context.Property.PropertyType)
            && context.BuildColumn.ValueConverter?.GetType().BaseType == typeof(MarkupBooleanConverterBase<string>))

            // Case 2: Atomic types
            || typeof(string).IsAssignableFrom(propertyTypeClean)
            || typeof(int).IsAssignableFrom(propertyTypeClean)
            || typeof(long).IsAssignableFrom(propertyTypeClean)
            || typeof(DateTime).IsAssignableFrom(propertyTypeClean)
            )
        {
            var builder = DataGridColumnBuilder.ForValuePath(context.Property.Name)
                .BasedOnAutoGridColumnContext(context);

            DataGridTextColumnBuilder textBuilder;
            if (buildTextColumn.IconSource is not null)
            {
                textBuilder = builder
                    .RenderAsTextWithImage(buildTextColumn.IconSource.IconPropertyPath)
                    .WithImageSize(buildTextColumn.IconSource.FixedSize)
                    .WithTextHidden(buildTextColumn.IconSource.HideText);
            }
            else
            {
                textBuilder = builder.RenderAsText();
            }

            if (buildTextColumn.TextHighlightingPatternPath is not null)
            {
                // Using the DataGrid.DataContext, previously attached via a resource
                var resource = context.DataGrid.GetBindingProxy();

                Binding[] baseBindings = [
                    new Binding("Data." + buildTextColumn.TextHighlightingPatternPath)
                    {
                        Source = resource
                    },
                    new Binding("Data." + buildTextColumn.TextHighlightingUseRegexPath)
                    {
                        Source = resource
                    }];
                textBuilder = textBuilder.WithTextHighlighting(baseBindings);
            }

            context.Args.Column = textBuilder.Build();
        }
    }
}
