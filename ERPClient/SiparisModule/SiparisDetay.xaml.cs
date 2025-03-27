using EntityLayer.DTOs;
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

namespace ERPClient.SiparisModule
{
    /// <summary>
    /// Interaction logic for SiparisDetay.xaml
    /// </summary>
    public partial class SiparisDetay : Window
    {
        private readonly OrderDetailDto _detail;


        public SiparisDetay(OrderDetailDto detail)
        {
            InitializeComponent();
            _detail = detail;
            LoadDetails();

        }

        // Sipariş geçmişi içerisinde bulunan sipariş detayları butonuna tıklanıldığında çağırılan kısım
        private void LoadDetails()
        {
            txtCustomerName.Text = _detail.Customer.CustomerName;
            txtCountry.Text = _detail.Customer.Country;
            txtTaxNo.Text = _detail.Customer.TaxNo;
            txtAddress.Text = _detail.Customer.Address;
            txtDeliveryAddress.Text = _detail.Customer.DeliveryAddress;
            txtPhone.Text = _detail.Customer.PhoneNumber;
            txtEmail.Text = _detail.Customer.Email;

            txtOrderDate.Text = _detail.OrderDate.ToShortDateString();

            var YurtIci = _detail.Customer.Country?.ToLower() == "Türkiye";
            var currencySymbol = YurtIci ? "₺" : "€";

            txtShippingFee.Text = currencySymbol + (_detail.ShippingFee ?? 0).ToString("N2");
            txtTotal.Text = currencySymbol + _detail.TotalPrice.ToString("N2");

            // Ürünler
            var items = _detail.OrderItems.Select(x => new
            {
                x.Product.ParcaAdi,
                x.Quantity,
                x.UnitPrice,
                Total = x.Quantity * x.UnitPrice
            }).ToList();

            dgOrderItems.ItemsSource = items;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
