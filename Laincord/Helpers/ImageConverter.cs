using System.Collections.Concurrent;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Laincord.Helpers
{
    public class ImageConverter : IValueConverter
    {
        private static readonly ConcurrentDictionary<string, BitmapImage> _cache = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return DependencyProperty.UnsetValue;
            else if (value is string uri)
            {
                if (string.IsNullOrWhiteSpace(uri)) return DependencyProperty.UnsetValue;

                if (_cache.TryGetValue(uri, out var cached))
                    return cached;

                var bitmap = new BitmapImage(new Uri(uri));
                if (bitmap.CanFreeze)
                {
                    bitmap.Freeze();
                    _cache[uri] = bitmap;
                }
                return bitmap;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}