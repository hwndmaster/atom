using System.Collections;

namespace Genius.Atom.UI.Forms.Controls.TagEditor;

public sealed class TagItemComparer : IComparer
{
    public int Compare(object? x, object? y)
    {
        var xTag = x as ITagItemViewModel;
        var yTag = y as ITagItemViewModel;

        if (xTag is null && yTag is null)
            return 0;
        if (xTag is null)
            return -1;
        if (yTag is null)
            return 1;

        return string.CompareOrdinal(xTag.Tag, yTag.Tag);
    }
}
