using BusinessLayer.Interfaces;
using DataAccessLayer.Data;
using EntityLayer.Entities;
using ERPClient.Model;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ERPClient.ViewModel
{
    public class WareHouseAddViewModel: BaseViewModel
    {
        private readonly User _currentUser;
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

        public WareHouseAddViewModel(IDepoService depo, User user)
        {
            _depo = depo;
            _currentUser = user;
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
                LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} yeni bir depo ekledi: {DepoAdi}");
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
                    LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} depo ekleme penceresini kapattı.");
                    break;
                }
            }
        }
    }
}
