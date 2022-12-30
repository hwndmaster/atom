using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.TagEditor;
using MahApps.Metro.Controls;

namespace Genius.Atom.UI.Forms.Wpf.Builders;

internal sealed class DataGridTagEditorColumnBuilder : DataGridColumnBuilder
{
    internal DataGridTagEditorColumnBuilder(DataGridColumnBuilder parentBuilder)
        : base(parentBuilder.NotNull())
    {
    }

    public override DataGridTemplateColumn Build()
    {
        var column = CreateColumn();
        var binding = new Binding(_valuePath);

        column.CellTemplate = CreateTagEditorDataTemplate(binding, @readonly: true);
        column.CellEditingTemplate = CreateTagEditorDataTemplate(binding, @readonly: false);

        return column;
    }

    private static DataTemplate CreateTagEditorDataTemplate(Binding bindToValue, bool @readonly)
    {
        var tagEditorFactory = new FrameworkElementFactory(typeof(TagEditor));
        tagEditorFactory.SetBinding(FrameworkElement.DataContextProperty, bindToValue);
        if (@readonly)
        {
            tagEditorFactory.SetValue(ControlsHelper.IsReadOnlyProperty, true);
        }
        return new DataTemplate { VisualTree = tagEditorFactory };
    }
}
