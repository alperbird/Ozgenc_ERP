using BusinessLayer.Interfaces;
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
    /// Interaction logic for GeneralStockAddWindow.xaml
    /// </summary>
    public partial class GeneralStockAddWindow : Window
    {
        private int? _selectedStockId;
        private readonly User _currentUser;
        public GeneralStockAddWindow(IStokService service, IDepoService depo, IParcalarService parca, ITedarikciService tedarikci, User user, int? id = null)
        {
            InitializeComponent();
            _currentUser = user;
            LogHelper.LogAction(user.Id, $"{user.Name} {user.Surname} Genel Parça Stok Ekleme sayfasına giriş yaptı.");
            DataContext = new GeneralStockAddViewModel(service, depo, parca, tedarikci, user, id);

        }


        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Genel Parça Stok Ekleme sayfasından çıkış yaptı.");
        }
    }
}
