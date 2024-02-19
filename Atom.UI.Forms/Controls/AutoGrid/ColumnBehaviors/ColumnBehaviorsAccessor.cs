namespace Genius.Atom.UI.Forms.Controls.AutoGrid.ColumnBehaviors;

internal static class ColumnBehaviorsAccessor
{
    private static readonly IAutoGridColumnBehavior[] _columnTypeChangers;
    private static readonly IAutoGridColumnBehavior[] _bindingChangers;
    private static readonly IAutoGridColumnBehavior[] _styleChangers;
    private static readonly IAutoGridColumnBehavior[] _miscBehaviors;

    static ColumnBehaviorsAccessor()
    {
        _columnTypeChangers = [
            new ColumnTextBehavior(),
            new ColumnTagEditorBehavior(),
            new ColumnButtonBehavior(),
            new ColumnToggleButtonBehavior(),
            new ColumnComboBoxBehavior(),
            new ColumnAttachedViewBehavior(),
        ];
        _bindingChangers = [
            new ColumnConverterBehavior(),
            new ColumnFormattingBehavior(),
            new ColumnNullableBehavior(),
        ];
        _styleChangers = [
            new ColumnTooltipBehavior(),
            new ColumnStylingBehavior(),
            new ColumnValidationBehavior(),
        ];
        _miscBehaviors = [
            new ColumnHeaderNameBehavior(),
            new ColumnReadOnlyBehavior(),
            new ColumnAutoWidthBehavior(),
            new ColumnDisplayIndexBehavior(),
            new ColumnVisibilityBehavior(),
        ];
    }

    public static IAutoGridColumnBehavior[] GetAll()
    {
        return [.._columnTypeChangers, .._bindingChangers, .._styleChangers, .._miscBehaviors];
    }

    public static IAutoGridColumnBehavior[] GetForDynamicColumn()
    {
        return [.._bindingChangers, .._styleChangers, .._miscBehaviors];
    }
}
