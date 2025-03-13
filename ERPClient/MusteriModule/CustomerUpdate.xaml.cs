using EntityLayer.DTOs;
using EntityLayer.Entities;
using ERPClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ERPClient.MusteriModule
{
    /// <summary>
    /// Interaction logic for CustomerUpdate.xaml
    /// </summary>
    public partial class CustomerUpdate : Page
    {
        private Model.Customer _selectedCustomer;
        private readonly HttpClient _client;

        private readonly string apiUrl = "http://192.168.5.81:5001/api/Customer";
        public CustomerUpdate(Model.Customer customer, HttpClient client)
        {
            InitializeComponent();
            _selectedCustomer = customer;
            _client = client;

            txtCustomerName.Text = _selectedCustomer.CustomerName;
            txtAddress.Text = _selectedCustomer.Address;
            txtDeliveryAddress.Text = _selectedCustomer.DeliveryAddress;
            txtContact.Text = _selectedCustomer.Contact;
            txtEmail.Text = _selectedCustomer.Email;
            txtPhoneNumber.Text = _selectedCustomer.PhoneNumber;
            txtTaxNo.Text = _selectedCustomer.TaxNo;

        }

        private async void btnUpdateSave_Click(object sender, RoutedEventArgs e)
        {
            var updateCustomer = new CustomerUpdateDto
            {
                Id = _selectedCustomer.Id,
                CustomerName = txtCustomerName.Text,
                Address = txtAddress.Text,
                DeliveryAddress = txtDeliveryAddress.Text,
                Contact = txtContact.Text,
                Email = txtEmail.Text,
                PhoneNumber = txtPhoneNumber.Text,
                TaxNo = txtTaxNo.Text,
                Date = _selectedCustomer.Date,
                LastUpdated = DateTime.Now
            };



            string jsonContent = System.Text.Json.JsonSerializer.Serialize(updateCustomer, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var customerContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage customerResponse = await _client.PutAsync($"{apiUrl}/{_selectedCustomer.Id}", customerContent);

            string responseContent = await customerResponse.Content.ReadAsStringAsync();

            if (customerResponse.IsSuccessStatusCode)
            {
                MessageBox.Show("Müşteri başarılı bir şekilde güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                Window.GetWindow(this)?.Close();
                
            }
            else
            {
                MessageBox.Show("Müşteri güncellenirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
