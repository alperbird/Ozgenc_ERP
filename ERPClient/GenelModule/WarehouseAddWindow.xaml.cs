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
    /// Interaction logic for WarehouseAddWindow.xaml
    /// </summary>
    public partial class WarehouseAddWindow : Window
    {
        private readonly User _currentUser;
        public WarehouseAddWindow(IDepoService service, User user)
        {
            InitializeComponent();
            _currentUser = user;
            DataContext = new WareHouseAddViewModel(service, user);
            LogHelper.LogAction(user.Id, $"{user.Name} {user.Surname}, Depo Ekleme penceresini açtı.");
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Depo Ekleme penceresini kapattı.");
        }
    }
}
