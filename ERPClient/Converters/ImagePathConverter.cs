using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ERPClient.Converters
{
    // Sadece sunucu bilgisayarının içindeki resimlere erişebiliyor. Bunun için kendi resim pathimizi ayarlamamız gerekebilir. Araştırıp güncelleyemeye çalışacağım.
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string defaultImagePath = "/Assets/image.png"; //Varsayılan resim

            if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
            {
                // Eğer gelen yol zaten mutlaksa direkt kullan
                if (Path.IsPathRooted(imagePath))
                {
                    return File.Exists(imagePath) ? new BitmapImage(new Uri(imagePath, UriKind.Absolute)) : new BitmapImage(new Uri(defaultImagePath, UriKind.Relative));
                }

                // Mutlak yol oluştur
                string basePath = @"C:\Ozgenc_ERP\Debug\net8.0\"; //Sunucu bilgisayarındaki ana dizin
                string fullPath = Path.Combine(basePath, imagePath);

                return File.Exists(fullPath) ? new BitmapImage(new Uri(fullPath, UriKind.Absolute)) : new BitmapImage(new Uri(defaultImagePath, UriKind.Relative));
            }

            return new BitmapImage(new Uri(defaultImagePath, UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
