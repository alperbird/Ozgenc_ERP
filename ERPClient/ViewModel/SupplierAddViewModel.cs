using BusinessLayer.Interfaces;
using EntityLayer.Entities;
using ERPClient.Model;
using System.Windows;
using System.Windows.Input;

namespace ERPClient.ViewModel
{
    public class SupplierAddViewModel : BaseViewModel
    {
        private readonly User _currentUser;
        private string _tedarikciAdi;

        public string tedarikciAdi
        {
            get => _tedarikciAdi;
            set
            {
                _tedarikciAdi = value;
                OnPropertyChanged(nameof(tedarikciAdi));
            }
        }

        private readonly ITedarikciService _tedarikci;

        public ICommand SaveCommand { get; }

        public SupplierAddViewModel(ITedarikciService tedarikci, User user)
        {
            _tedarikci = tedarikci;
            SaveCommand = new RelayCommand<object>(SavePart);
            _currentUser = user;
        }

        private async void SavePart(object obj)
        {
            if (string.IsNullOrWhiteSpace(tedarikciAdi))
            {
                MessageBox.Show("Tedarikçi adı boş olamaz.");
                return;
            }
            var newSupplier = new F_Supplier { F_TedarikciAdi = tedarikciAdi };
            await _tedarikci.AddTedarikci(newSupplier);
            MessageBox.Show("Tedarikçi başarıyla eklendi.");
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} yeni bir tedarikçi ekledi: {tedarikciAdi}");
            CloseWindow();
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = true;
                    window.Close();
                    LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} tedarikçi ekleme penceresini kapattı.");
                    break;
                }
            }
            
        }
    }
}
