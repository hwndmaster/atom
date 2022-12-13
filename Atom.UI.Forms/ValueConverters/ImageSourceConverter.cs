using System.Windows.Data;

namespace Genius.Atom.UI.Forms;

internal sealed class ImageSourceConverter : IValueConverter
{
    private readonly Dictionary<int, object> _cache = new();

    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        if (value is string resourceName)
        {
            if (resourceName.Length == 0)
                return null;

            var hash = resourceName.GetHashCode();
            if (!_cache.TryGetValue(hash, out var imageSource))
            {
                imageSource = Application.Current.FindResource(resourceName);
                _cache.Add(hash, imageSource);
            }

            return imageSource;
        }

        return value;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
