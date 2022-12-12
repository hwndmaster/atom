namespace Genius.Atom.UI.Forms;

public sealed class InvertedBooleanConverter : MarkupBooleanConverterBase<bool>
{
    public InvertedBooleanConverter()
        : base(false, true) { }
}
