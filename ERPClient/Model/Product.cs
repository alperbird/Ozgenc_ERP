using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ERPClient.Model
{
    public class Product
    {
        public int Id { get; set; }

        [NotMapped]
        public string MakineAdi { get; set; } = string.Empty;
        public string? Sinif { get; set; }
        public string? ResimYolu { get; set; }
        public byte[]? ResimData { get; set; }
        public string? ParcaAdi { get; set; }
        public decimal FiyatTL { get; set; }
        public decimal FiyatEUR { get; set; }
        public string? Aciklama { get; set; }

        public int Stok { get; set; }
        [JsonIgnore]
        public List<ProductMachine>? ProductMachines { get; set; }

        public string MakineAdlari => ProductMachines != null
        ? string.Join(", ", ProductMachines.Select(pm => pm.Machine?.MakineAdi).Where(n => !string.IsNullOrEmpty(n)))
        : string.Empty;



        public BitmapImage? ThumbnailImage
        {
            get
            {
                if (ResimData == null || ResimData.Length == 0)
                {
                    
                    var uri = new Uri("pack://application:,,,/Assets/image.png");
                    return new BitmapImage(uri);
                }

                using var mem = new MemoryStream(ResimData);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = mem;
                image.DecodePixelWidth = 50;
                image.EndInit();
                return image;
            }
        }
    }
}
