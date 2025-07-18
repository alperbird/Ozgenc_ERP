using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Interfaces;
using EntityLayer.Entities;
using System.Windows;
using System.Windows.Input;
using ERPClient.Model;

namespace ERPClient.ViewModel
{
    public class GeneralPartAddViewModel: BaseViewModel
    {
        private readonly User _currentUser;
        private string _malzemeAdi;

        public string malzemeAdi
        {
            get => _malzemeAdi;
            set
            {
                _malzemeAdi = value;
                OnPropertyChanged(nameof(malzemeAdi));
            }
        }

        private readonly IParcalarService _parcaService;

        public ICommand SaveCommand { get; }

        public GeneralPartAddViewModel(IParcalarService parcaService, User user)
        {
            _parcaService = parcaService;
            SaveCommand = new RelayCommand<object>(SavePart);
            _currentUser = user;
        }

        private async void SavePart(object obj)
        {
            if (string.IsNullOrWhiteSpace(malzemeAdi))
            {
                MessageBox.Show("Parça adı boş olamaz.");
                return;
            }

            var newPart = new F_GeneralPart { F_MalzemeAdi = malzemeAdi };
            await _parcaService.AddParca(newPart);

            MessageBox.Show("Parça başarıyla eklendi.");
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} yeni bir parça ekledi: {malzemeAdi}");
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
                    LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} parça ekleme penceresini kapattı.");
                    break;
                }
            }
        }
    }
}
