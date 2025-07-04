using BusinessLayer.Interfaces;
using EntityLayer.Entities;
using System.Windows;
using System.Windows.Input;

namespace ERPClient.ViewModel
{
    public class SupplierAddViewModel : BaseViewModel
    {
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

        public SupplierAddViewModel(ITedarikciService tedarikci)
        {
            _tedarikci = tedarikci;
            SaveCommand = new RelayCommand<object>(SavePart);
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
                    break;
                }
            }
        }
    }
}
