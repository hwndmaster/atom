using System.Linq.Expressions;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal record IconSourceRecord(string IconPropertyPath, double? FixedSize = null, bool HideText = false);
public record IconSourceRecord<TViewModel>(Expression<Func<TViewModel, string?>> IconPropertyPath, double? FixedSize = null, bool HideText = false);
