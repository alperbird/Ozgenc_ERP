using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPClient.Model
{
    public class ProductOrderRow: INotifyPropertyChanged
    {
        private string _selectedMachine;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
        public string? CustomProductName { get; set; }

        public ObservableCollection<string> AvailableMachines { get; set; } = new();

        public string? SelectedMachine
        {
            get => _selectedMachine;
            set
            {
                if (_selectedMachine != value)
                {
                    _selectedMachine = value;
                    OnPropertyChanged(nameof(SelectedMachine));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string v)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
        }

        //Açıklama: Burası sadece ve sadece YurtDisi ve YurtIci siparişlerinde ekle butonuna basıldığında referans alınacak olan model sınıfıdır.
    }
}
