using EntityLayer.Entities;
using ERPClient.Model;
using ERPClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ERPClient.GenelModule
{
    /// <summary>
    /// Interaction logic for GenelParcaSayfasi.xaml
    /// </summary>
    public partial class GenelParcaSayfasi : Window
    {
        private readonly User _currentUser;
        public GenelParcaSayfasi(User user)
        {
            _currentUser = user;
            InitializeComponent();

            //var stokService = App.StokService;
            //var depoService = App.DepoService;
            //var parcaService = App.ParcalarService;
            //var tedarikciService = App.TedarikciService;

            if (App.StokService == null || App.DepoService == null || App.ParcalarService == null || App.TedarikciService == null)
            {
                MessageBox.Show("Servisler çözülmedi. App.xaml.cs içinde servisleri düzgün başlattın mı?");
                return;
            }


            // ViewModel'i oluştur ve bağla
            this.DataContext = new GeneralStockViewModel(App.StokService, App.DepoService, App.ParcalarService, App.TedarikciService);
        }



        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Genel Parça sayfasından çıkış yaptı.");
            this.Close();
        }
    }
}
