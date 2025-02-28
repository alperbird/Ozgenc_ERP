using ERPClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace ERPClient.YedekParcaModule
{
    /// <summary>
    /// Interaction logic for YedekParcaUpdateWindow.xaml
    /// </summary>
    public partial class YedekParcaUpdateWindow : Window
    {
        public YedekParcaUpdateWindow(Product selectedProduct, HttpClient client)
        {
            InitializeComponent();

            contentFrame.Content = new YedekParcaUpdate(selectedProduct, client);
        }


    }
}
