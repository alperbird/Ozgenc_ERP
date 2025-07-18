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
    /// Interaction logic for GeneralPartAddWindow.xaml
    /// </summary>
    public partial class GeneralPartAddWindow : Window
    {
        private readonly User _currentUser;
        public GeneralPartAddWindow(IParcalarService parca, User user)
        {
            _currentUser = user;
            InitializeComponent();
            LogHelper.LogAction(user.Id, $"{user.Name} {user.Surname} Genel Parça Ekleme sayfasına giriş yaptı.");
            DataContext = new GeneralPartAddViewModel(parca, user);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Genel Parça Ekleme sayfasından çıkış yaptı.");
        }
    }
}
