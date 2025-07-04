using BusinessLayer.Interfaces;
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
        public SupplierAddWindow(ITedarikciService tedarikci)
        {
            InitializeComponent();
            DataContext = new SupplierAddViewModel(tedarikci);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
