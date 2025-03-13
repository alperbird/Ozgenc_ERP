using EntityLayer.DTOs;
using EntityLayer.Entities;
using ERPClient.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ERPClient.MusteriModule
{
    /// <summary>
    /// Interaction logic for Customer.xaml
    /// </summary>
    public partial class Customer : Window
    {
        private readonly HttpClient _client = new HttpClient();
        private const string apiUrl = "http://192.168.5.81:5001/api/Customer";


        public Customer()
        {
            InitializeComponent();
            //dgMusteri.ItemsSource = _customer;
            LoadTable();
        }

        private async void LoadTable()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                List<CustomerCreateDto> customers = JsonConvert.DeserializeObject<List<CustomerCreateDto>>(jsonResponse) ?? new List<CustomerCreateDto>();

                Dispatcher.Invoke(() =>
                {
                    if (dgMusteri.ItemsSource != null)
                    {
                        dgMusteri.ItemsSource = null; // 🔥 Önce null yap, veri değişimini UI'ye bildir
                    }

                    dgMusteri.ItemsSource = customers; // 🔥 Veri değişimi artık tekrar çağrı tetiklemeyecek
                });
            }
            catch (Exception) { }
        }

        #region CRUD İşlemleri
        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var newCustomer = new CustomerCreateDto
            {
                CustomerName = txtCustomer.Text,
                Address = txtAddress.Text,
                DeliveryAddress = txtDeliveryAddress.Text,
                Contact = txtContact.Text,
                Email = txtEmail.Text,
                PhoneNumber = txtPhoneNumber.Text,
                TaxNo = txtTaxNo.Text,
                Date = DateTime.Now
            };

            string json = JsonConvert.SerializeObject(newCustomer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var createdCustomer = JsonConvert.DeserializeObject<CustomerCreateDto>(responseBody);
                    MessageBox.Show($"Müşteri başarılı bir şekilde eklendi. Müşteri Id: {createdCustomer.Id}", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadTable();
                    CleanForm();
                }
                else
                {
                    MessageBox.Show("Müşteri eklenirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Hata: "+ ex.Message, "Bağlantı hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgMusteri_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #endregion

        #region Pagination İşlemleri
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e) //sayfadan çıkış butonu
        {
            this.Close();
        }

        private void CleanForm()
        {
            txtCustomer.Text = "";
            txtAddress.Text = "";
            txtDeliveryAddress.Text = "";
            txtContact.Text = "";
            txtEmail.Text = "";
            txtPhoneNumber.Text = "";
            txtTaxNo.Text = "";
        }

    }
}
