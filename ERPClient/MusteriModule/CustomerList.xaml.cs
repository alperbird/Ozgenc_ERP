using EntityLayer.DTOs;
using EntityLayer.Entities;
using ERPClient.Model;
using ERPClient.YedekParcaModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Text.Json;
using Country = ERPClient.Model.Country;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Diagnostics;


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

        private ObservableCollection<Model.Customer> paginatedCustomers = new ObservableCollection<Model.Customer>();
        private int currentPage = 1;
        private int pageSize = 10;


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

                // Pagination’a uygun modele çevir
                allCustomer = new ObservableCollection<Model.Customer>(
                    customers.Select(dto => new Model.Customer
                    {
                        Id = dto.Id,
                        CustomerName = dto.CustomerName,
                        CountryId = dto.CountryId,
                        CountryName = dto.CountryName,
                        Address = dto.Address,
                        DeliveryAddress = dto.DeliveryAddress,
                        Contact = dto.Contact,
                        Email = dto.Email,
                        PhoneNumber = dto.PhoneNumber,
                        TaxNo = dto.TaxNo,
                        Date = dto.Date,
                        LastUpdated = dto.LastUpdated
                    })
                );

                UpdatePagination(); // Sayfalı listeyi bastır

            }
            catch (Exception ex)
            {
                MessageBox.Show("Müşteri listesi çekilirken hata oluştu!\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            if (dgMusteri.SelectedItem is Model.Customer dto)
            {
                var selectedCustomer = new Model.Customer
                {
                    Id = dto.Id,
                    CustomerName = dto.CustomerName,
                    CountryId = dto.CountryId,
                    CountryName = dto.CountryName,
                    Address = dto.Address,
                    DeliveryAddress = dto.DeliveryAddress,
                    Contact = dto.Contact,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    TaxNo = dto.TaxNo,
                    Date = dto.Date,
                    LastUpdated = dto.LastUpdated
                };

                var updateWindow = new CustomerUpdateWindow(selectedCustomer, _client);
                updateWindow.ShowDialog();

                LoadTable();
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir müşteri seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            //if (dgMusteri.SelectedItem is Model.Customer selectedCustomer)
            //{
            //    MessageBox.Show($"Seçilen Müşteri: {selectedCustomer.CustomerName}", "Seçildi", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
        }

        #endregion

        #region Pagination İşlemleri
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdatePagination();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if ((currentPage * pageSize) < allCustomer.Count)
            {
                currentPage++;
                UpdatePagination();
            }
        }

        private void UpdatePagination()
        {
            if (allCustomer.Count == 0) return; // Eğer hiç veri yoksa pagination çalıştırma

            int totalItems = allCustomer.Count;
            int totalPages = (totalItems + pageSize - 1) / pageSize;
            int skip = (currentPage - 1) * pageSize;

            paginatedCustomers.Clear();
            foreach (var item in allCustomer.Skip(skip).Take(pageSize))
            {
                paginatedCustomers.Add(item);
            }

            dgMusteri.ItemsSource = paginatedCustomers;
            txtPageInfo.Text = $"Sayfa {currentPage} / {totalPages}";

            btnPrevious.IsEnabled = currentPage > 1;
            btnNext.IsEnabled = currentPage < totalPages;
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

        #region Combobox Ülke Doldurma
        // Ülke seçtirioz
        private void cmbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //Ülke combobox doldurma
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
        #endregion

        private void btnAddCountry_Click(object sender, RoutedEventArgs e)
        {
            var country = new CountryCRUDWindow(_client);
            country.ShowDialog();

            LoadCountries();
        }

        private async void btnTopluMusteriEkle_Click(object sender, RoutedEventArgs e)
        {
            ExcelPackage.License.SetNonCommercialPersonal("slim shady");

            // 1️⃣ Ülke listesini çek
            var countries = await GetAllCountriesAsync();
            if (countries.Count == 0)
            {
                MessageBox.Show("Ülke listesi alınamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2️⃣ Excel dosyasını seçtir
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Excel Dosyası|*.xlsx" };
            if (dialog.ShowDialog() != true) return;

            var customers = new List<CustomerCreateDto>();
            using var package = new ExcelPackage(new FileInfo(dialog.FileName));
            var sheet = package.Workbook.Worksheets[0];
            int rowCount = sheet.Dimension.Rows;

            // 3️⃣ Satır satır müşteri oluştur
            for (int row = 2; row <= rowCount; row++)
            {
                string customerName = sheet.Cells[row, 1].Text.Trim();
                string countryName = sheet.Cells[row, 2].Text.Trim();
                int countryId = countries.FirstOrDefault(c => c.CountryName == countryName)?.Id ?? 0;
                string address = sheet.Cells[row, 3].Text.Trim();
                string deliveryAddress = sheet.Cells[row, 4].Text.Trim();
                string contact = sheet.Cells[row, 5].Text.Trim();
                string email = sheet.Cells[row, 6].Text.Trim();
                string phone = sheet.Cells[row, 7].Text.Trim();
                string rawTaxNo = sheet.Cells[row, 8].Text.Trim();
                string taxNo = (string.IsNullOrWhiteSpace(rawTaxNo) || rawTaxNo == "0" || rawTaxNo.ToLower() == "nan") ? "" : rawTaxNo;


                var customer = new CustomerCreateDto
                {
                    CustomerName = customerName,
                    CountryId = countryId,
                    Address = address,
                    DeliveryAddress = deliveryAddress,
                    Contact = contact,
                    Email = email,
                    PhoneNumber = phone,
                    TaxNo = taxNo,
                    Date = DateTime.Now,
                    LastUpdated = DateTime.Now
                };

                customers.Add(customer);
            }

            // 4️⃣ API'ye gönder
            int basarili = 0;
            foreach (var c in customers)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(c);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("http://192.168.5.81:5001/api/Customer", content);
                if (response.IsSuccessStatusCode) basarili++;
            }

            // 5️⃣ Sonuç bildir
            MessageBox.Show($"{basarili} müşteri başarıyla eklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string CleanTaxNo(string taxNo)
        {
            if (string.IsNullOrWhiteSpace(taxNo)) return "";
            var cleaned = taxNo.Trim().ToLower();
            return (cleaned == "0" || cleaned == "nan") ? "" : taxNo.Trim();
        }

        private async Task<List<Country>> GetAllCountriesAsync()
        {
            var response = await _client.GetAsync("http://192.168.5.81:5001/api/Country");
            if (!response.IsSuccessStatusCode) return new List<Country>();

            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Country>>(json) ?? new List<Country>();
        }

    }
}
