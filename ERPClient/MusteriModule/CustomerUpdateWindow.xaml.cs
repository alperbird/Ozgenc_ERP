using System;
using ERPClient.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ERPClient.MusteriModule
{
    /// <summary>
    /// Interaction logic for CustomerUpdateWindow.xaml
    /// </summary>
    public partial class CustomerUpdateWindow : Window
    {
        public CustomerUpdateWindow(Customer selectedCustomer, HttpClient client)
        {
            InitializeComponent();
            contentFrame.Content = new CustomerUpdate(selectedCustomer, client);
        }
    }
}
