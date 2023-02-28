using System.Windows.Documents;
using Genius.Atom.Reporting.RichDocuments;

namespace Genius.Atom.Reporting.UI.RichDocuments;

public interface IFlowDocumentConverter
{
    FlowDocument Convert(RichDocument document);
}

internal sealed class FlowDocumentConverter : IFlowDocumentConverter
{
    private readonly IRichBlockConverter[] _converters;

    public FlowDocumentConverter(IEnumerable<IRichBlockConverter> converters)
    {
        _converters = converters.NotNull().ToArray();
    }

    public FlowDocument Convert(RichDocument document)
    {
        var resultingDocument = new FlowDocument();
        resultingDocument.Blocks.AddRange(document.Blocks.Select(x => ConvertBlock(x)));

        return resultingDocument;
    }

    private Block ConvertBlock(RichBlock block)
    {
        foreach (var converter in _converters)
        {
            if (converter.CanConvert(block))
            {
                return converter.Convert(block);
            }
        }

        throw new NotSupportedException($"The block of type {block.GetType().Name} is not supported.");
    }
}
