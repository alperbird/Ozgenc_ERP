using BusinessLayer.Interfaces;
using DataAccessLayer.Data;
using EntityLayer.Entities;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ERPClient.ViewModel
{
    public class WareHouseAddViewModel: BaseViewModel
    {
        private readonly IDepoService _depo;
        private string _depoAdi;

        public string DepoAdi
        {
            get => _depoAdi;
            set
            {
                _depoAdi = value;
                OnPropertyChanged(nameof(DepoAdi));
            }
        }

        public ICommand SaveCommand { get; set; }

        public WareHouseAddViewModel(IDepoService depo)
        {
            _depo = depo;
            SaveCommand = new RelayCommand<object>(SaveWareHouse);
        }

        private async void SaveWareHouse(object obj)
        {
            if (string.IsNullOrWhiteSpace(DepoAdi))
            {
                MessageBox.Show("Depo adı boş olamaz.");
                return;
            }

            var newWarehouse = new F_Warehouse { DepoAdi = DepoAdi };

            try
            {
                await _depo.AddWarehouse(newWarehouse);
                MessageBox.Show("Yeni kayıt başarıyla eklendi.");
                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt eklenirken hata oluştu:\n {ex.Message}");
            }
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = true;
                    window.Close();
                    break;
                }
            }
        }
    }
}
