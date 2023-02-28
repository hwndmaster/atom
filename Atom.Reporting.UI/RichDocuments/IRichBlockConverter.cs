using System.Windows.Documents;
using Genius.Atom.Reporting.RichDocuments;

namespace Genius.Atom.Reporting.UI.RichDocuments;

public interface IRichBlockConverter
{
    bool CanConvert(RichBlock block);
    Block Convert(RichBlock block);
}
