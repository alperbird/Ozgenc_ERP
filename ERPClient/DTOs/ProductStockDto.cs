using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPClient.DTOs
{
    public class ProductStockDto : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string ParcaAdi { get; set; } = string.Empty;

        private int _stok;
        public int Stok
        {
            get => _stok;
            set
            {
                if (_stok != value)
                {
                    _stok = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Stok)));
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
