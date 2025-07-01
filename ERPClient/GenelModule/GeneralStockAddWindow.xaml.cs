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
    /// Interaction logic for GeneralStockAddWindow.xaml
    /// </summary>
    public partial class GeneralStockAddWindow : Window
    {
        public GeneralStockAddWindow(IStokService service, IDepoService depo, IParcalarService parca, ITedarikciService tedarikci)
        {
            InitializeComponent();
            DataContext = new GeneralStockAddViewModel(service, depo, parca, tedarikci);
        }


        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
