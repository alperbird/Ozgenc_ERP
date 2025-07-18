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
    /// Interaction logic for SupplierAddWindow.xaml
    /// </summary>
    public partial class SupplierAddWindow : Window
    {
        private readonly User _currentuser;
        public SupplierAddWindow(ITedarikciService tedarikci, User user)
        {
            InitializeComponent();
            _currentuser = user;
            LogHelper.LogAction(user.Id, $"{user.Name} {user.Surname}, Tedarikçi Ekleme penceresini açtı.");
            DataContext = new SupplierAddViewModel(tedarikci, user);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            LogHelper.LogAction(_currentuser.Id, $"{_currentuser.Name} {_currentuser.Surname} Tedarikçi Ekleme penceresini kapattı.");
        }
    }
}
