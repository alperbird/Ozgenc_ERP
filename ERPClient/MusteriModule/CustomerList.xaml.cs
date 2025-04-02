using EntityLayer.DTOs;
using EntityLayer.Entities;
using ERPClient.Model;
using ERPClient.YedekParcaModule;
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
using Country = ERPClient.Model.Country;

namespace ERPClient.MusteriModule
{
    /// <summary>
    /// Interaction logic for Customer.xaml
    /// </summary>
    public partial class CustomerList : Window
    {

        private readonly HttpClient _client = new HttpClient();
        private const string apiUrl = "http://192.168.5.81:5001/api/Customer";
        private Model.Customer? _selectedCustomer;
        private ObservableCollection<Model.Customer> allCustomer = new ObservableCollection<Model.Customer>();


        public CustomerList()
        {
            InitializeComponent();
            //dgMusteri.ItemsSource = _customer;
            LoadTable();
            LoadCountries();
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
                        dgMusteri.ItemsSource = null; //Önce null yap, veri değişimini UI'ye bildir
                    }

                    dgMusteri.ItemsSource = customers; //Veri değişimi artık tekrar çağrı tetiklemeyecek
                });
            }
            catch (Exception) { }
        }

        #region CRUD İşlemleri
        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Country? selectedCountry = cmbCountry.SelectedItem as Country;
            if (selectedCountry == null)
            {
                MessageBox.Show("ÜLKE SEÇİMİ YAP!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            var newCustomer = new CustomerCreateDto
            {
                CustomerName = txtCustomer.Text,
                CountryId = selectedCountry.Id,
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
            if (dgMusteri.SelectedItem == null)
            {
                MessageBox.Show("Lütfen güncellenecek müşteriyi seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dgMusteri.SelectedItem is EntityLayer.DTOs.CustomerCreateDto dto)
            {
                var selectedCustomer = new ERPClient.Model.Customer
                {
                    Id = dto.Id,
                    CustomerName = dto.CustomerName,
                    //Country = dto.CountryId,
                    Address = dto.Address,
                    DeliveryAddress = dto.DeliveryAddress,
                    Contact = dto.Contact,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    TaxNo = dto.TaxNo,
                    Date = dto.Date
                };

                var updateWindow = new CustomerUpdateWindow(selectedCustomer, _client);
                updateWindow.ShowDialog();

                LoadTable();
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir ürün seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgMusteri.SelectedItem is EntityLayer.DTOs.CustomerCreateDto selectedCustomer)
            {
                var result = MessageBox.Show($"Müşteri '{selectedCustomer.CustomerName}' silinecek! Onaylıyor musunuz?",
                                             "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    HttpResponseMessage response = await _client.DeleteAsync($"{apiUrl}/{selectedCustomer.Id}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Müşteri başarıyla silindi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTable();
                    }
                    else
                    {
                        MessageBox.Show("Müşteri silinirken bir hata oluştu!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir müşteri seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dgMusteri_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMusteri.SelectedItem is Model.Customer selectedCustomer)
            {
                MessageBox.Show($"Seçilen Müşteri: {selectedCustomer.CustomerName}", "Seçildi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
            cmbCountry.SelectedItem = null;
            txtAddress.Text = "";
            txtDeliveryAddress.Text = "";
            txtContact.Text = "";
            txtEmail.Text = "";
            txtPhoneNumber.Text = "";
            txtTaxNo.Text = "";
        }

        // Ülke seçtirioz
        private void cmbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        // Gerekli ülkeleri ekleyin pls
        private async void LoadCountries()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("http://192.168.5.81:5001/api/Country");
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var countries = JsonConvert.DeserializeObject<List<Country>>(jsonResponse) ?? new List<Country>();

                cmbCountry.ItemsSource = countries;
                cmbCountry.DisplayMemberPath = "CountryName";
                cmbCountry.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ülke listesi çekilirken bir hata oluştu! " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddCountry_Click(object sender, RoutedEventArgs e)
        {
            var country = new CountryCRUDWindow(_client);
            country.ShowDialog();

            LoadCountries();
        }
    }
}
