using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace Genius.Atom.UI.Forms.Wpf.Builders;

internal class DataGridTextColumnBuilder : DataGridColumnBuilder
{
    private Binding[]? _textHighlightingBaseBindings;

    internal DataGridTextColumnBuilder(DataGridColumnBuilder parentBuilder)
        : base(parentBuilder.NotNull())
    {
    }

    public DataGridTextColumnBuilder WithTextHighlighting(Binding[] baseBindings)
    {
        _textHighlightingBaseBindings = baseBindings;
        return this;
    }

    internal FrameworkElementFactory RenderTextBlock()
    {
        var binding = CreateBinding();

        if (_textHighlightingBaseBindings is not null)
        {
            System.Diagnostics.Debug.Assert(_textHighlightingBaseBindings.Length == 2);

            var completeBinding = new MultiBinding
            {
                Converter = new HighlightedTextConverter()
            };
            completeBinding.Bindings.Add(_textHighlightingBaseBindings[0]); // Search Text
            completeBinding.Bindings.Add(_textHighlightingBaseBindings[1]); // Search UseRegex
            completeBinding.Bindings.Add(binding);

            FrameworkElementFactory contentControl = new(typeof(ContentControl));
            contentControl.SetBinding(ContentControl.ContentProperty, completeBinding);

            return contentControl;
        }
        else
        {
            var textFactory = new FrameworkElementFactory(typeof(TextBlock));
            textFactory.SetBinding(TextBlock.TextProperty, binding);
            return textFactory;
        }
    }

    public override DataGridTemplateColumn Build()
    {
        var column = CreateColumn();
        var textBlock = RenderTextBlock();
        column.CellTemplate = new DataTemplate { VisualTree = textBlock };
        column.CellEditingTemplate = CreateTextBoxTemplate(CreateBinding());
        return column;
    }

    private static DataTemplate CreateTextBoxTemplate(Binding bindToValue)
    {
        var textFactory = new FrameworkElementFactory(typeof(TextBox));
        textFactory.SetBinding(TextBox.TextProperty, bindToValue);
        return new DataTemplate { VisualTree = textFactory };
    }

    // TODO: Cover with unit tests
    private sealed class HighlightedTextConverter : IMultiValueConverter
    {
        private readonly record struct Match(int Index, int Length);
        private static Dictionary<int, Regex?> _regexCache = new();
        private static Lazy<Style> _runHighlightStyle = new(() => (Style)Application.Current.Resources["Atom.Run.Highlight"]);

        public HighlightedTextConverter()
        {
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var pattern = (string?)values[0];
            var useRegex = (bool)values[1];
            var text = values[2] as string;

            if (text is null)
                return null!;

            List<Match>? matches = null;

            var textBlock = new TextBlock();

            if (useRegex)
            {
                var regex = GetOrCreateRegex(pattern);
                if (regex is not null)
                {
                    matches = regex.Matches(text).Select(x => new Match(x.Index, x.Length)).ToList();
                }
            }
            else
            {
                matches = ExtractMatches(text, pattern).ToList();
            }

            if (matches is null || matches.Count == 0)
            {
                textBlock.Text = text;
            }
            else
            {
                for (var matchIndex = 0; matchIndex < matches.Count; matchIndex++)
                {
                    var match = matches[matchIndex];
                    if (match.Index > 0)
                    {
                        var startIndex = matchIndex == 0 ? 0 : matches[matchIndex - 1].Index + matches[matchIndex - 1].Length;
                        textBlock.Inlines.Add(new Run(text.Substring(startIndex, matches[matchIndex].Index - startIndex)));
                    }
                    textBlock.Inlines.Add(new Run(text.Substring(match.Index, match.Length))
                    {
                        Style = _runHighlightStyle.Value
                    });
                }

                var lastStartIndex = matches[^1].Index + matches[^1].Length;
                if (lastStartIndex != text.Length - 1)
                {
                    textBlock.Inlines.Add(new Run(text.Substring(lastStartIndex)));
                }
            }

            return textBlock;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This converter cannot be used in two-way binding.");
        }

        private static Regex? GetOrCreateRegex(string? pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return null;

            var key = pattern.GetHashCode();

            if (_regexCache.TryGetValue(key, out var regex))
            {
                return regex;
            }

            try
            {
                regex = new Regex(pattern, RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                // Regex is invalid, just ignore
            }

            _regexCache.Add(key, regex);

            return regex;
        }

        private static IEnumerable<Match> ExtractMatches(string text, string? pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                yield break;
            }

            int index = 0;
            while (true)
            {
                index = text.IndexOf(pattern, index, StringComparison.InvariantCultureIgnoreCase);
                if (index == -1)
                    yield break;
                yield return new Match(index, pattern.Length);
                index++;
            }
        }
    }
}
