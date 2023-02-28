using System.Windows.Documents;
using Genius.Atom.Reporting.RichDocuments;

namespace Genius.Atom.Reporting.UI.RichDocuments;

public interface IInlineRichBlockConverter
{
    bool CanConvert(InlineRichBlock block);
    Inline Convert(InlineRichBlock block);
}
