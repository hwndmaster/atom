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

        if (
            // Case 1:
            (typeof(bool).IsAssignableFrom(context.Property.PropertyType)
            && context.BuildColumn.ValueConverter?.GetType().BaseType == typeof(MarkupBooleanConverterBase<string>))

            // Case 2:
            || typeof(string).IsAssignableFrom(context.Property.PropertyType))
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
                var resource = context.DataGrid.FindResource("proxy");

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
