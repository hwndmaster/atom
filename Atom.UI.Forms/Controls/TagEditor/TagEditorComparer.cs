using System.Collections;

namespace Genius.Atom.UI.Forms.Controls.TagEditor;

public sealed class TagEditorComparer : IComparer
{
    private readonly TagItemComparer _tagItemComparer = new();

    public int Compare(object? x, object? y)
    {
        var xTagEditor = x as ITagEditorViewModel;
        var yTagEditor = y as ITagEditorViewModel;

        if (xTagEditor is null && yTagEditor is null)
            return 0;
        if (xTagEditor is null)
            return -1;
        if (yTagEditor is null)
            return 1;

        var tagsX = xTagEditor.SelectedTags.OrderBy(x => x.Tag).ToArray();
        var tagsY = yTagEditor.SelectedTags.OrderBy(y => y.Tag).ToArray();

        var maxCount = Math.Max(tagsX.Length, tagsY.Length);

        for (var i = 0; i < maxCount; i++)
        {
            var xTag = tagsX.ElementAtOrDefault(i);
            var yTag = tagsY.ElementAtOrDefault(i);

            var compareResult = _tagItemComparer.Compare(xTag, yTag);
            if (compareResult != 0)
                return compareResult;
        }

        return 0;
    }
}
