using EntityLayer.Entities;
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
        }



        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
