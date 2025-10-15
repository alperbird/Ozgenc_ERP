using EntityLayer.DTOs;
using ERPClient.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Country = ERPClient.Model.Country;
using User = EntityLayer.Entities.User;

namespace ERPClient.MusteriModule
{
    public partial class CustomerUpdate : Page
    {
        private readonly Model.Customer _selectedCustomer;
        private readonly User _currentUser;
        private readonly HttpClient _client;
        private readonly string apiUrl = "http://192.168.5.81:5001/api/Customer";

        public CustomerUpdate(Model.Customer customer, HttpClient client, User user)
        {
            InitializeComponent();
            _selectedCustomer = customer;
            _client = client;
            _currentUser = user;
            _ = InitForm();            

        }

        private async Task LoadCountries()
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

                // Mevcut müşterinin ülkesini seçili hale getir
                cmbCountry.SelectedValue = _selectedCustomer.CountryId; 

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ülke listesi çekilirken bir hata oluştu! " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task InitForm()
        {
            await LoadCountries();

            //Atamaları deneme amaçlı buraya taşıdım
            txtCustomerName.Text = _selectedCustomer.CustomerName;
            cmbCountry.Text = _selectedCustomer.CountryName;
            txtAddress.Text = _selectedCustomer.Address;
            txtDeliveryAddress.Text = _selectedCustomer.DeliveryAddress;
            txtContact.Text = _selectedCustomer.Contact;
            txtEmail.Text = _selectedCustomer.Email;
            txtPhoneNumber.Text = _selectedCustomer.PhoneNumber;
            txtTaxNo.Text = _selectedCustomer.TaxNo;
            txtTaxOffice.Text = _selectedCustomer.TaxOffice;

            cmbCountry.SelectedValue = _selectedCustomer.CountryId;
        }

        private async void btnUpdateSave_Click(object sender, RoutedEventArgs e)
        {
            // Seçilen ülkeyi yakala
            if (cmbCountry.SelectedValue is null)
            {
                MessageBox.Show("Lütfen bir ülke seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int selectedCountryId = (int)cmbCountry.SelectedValue;
            var selectedCountry = cmbCountry.SelectedItem as Country;

            var updateCustomer = new CustomerUpdateDto
            {
                Id = _selectedCustomer.Id,
                CustomerName = txtCustomerName.Text,
                CountryId = selectedCountryId,
                Address = txtAddress.Text,
                DeliveryAddress = txtDeliveryAddress.Text,
                Contact = txtContact.Text,
                Email = txtEmail.Text,
                PhoneNumber = txtPhoneNumber.Text,
                TaxNo = txtTaxNo.Text,
                TaxOffice = txtTaxOffice.Text,
                Date = _selectedCustomer.Date,
                LastUpdated = DateTime.Now,

                Country = new EntityLayer.Entities.Country
                {
                    Id = selectedCountry.Id,
                    CountryName = selectedCountry.CountryName
                }

            };

            string jsonContent = System.Text.Json.JsonSerializer.Serialize(updateCustomer, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            var customerContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage customerResponse = await _client.PutAsync($"{apiUrl}/{_selectedCustomer.Id}", customerContent);

            if (customerResponse.IsSuccessStatusCode)
            {
                MessageBox.Show("Müşteri başarılı bir şekilde güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                var old = _selectedCustomer;
                StringBuilder sb = new();
                sb.AppendLine($"{_currentUser.Name} {_currentUser.Surname}, {old.CustomerName} adlı müşteriyi güncelledi.");

                if (old.CustomerName != updateCustomer.CustomerName)
                    sb.AppendLine($"Müşteri adı: {old.CustomerName} -> {updateCustomer.CustomerName}");

                if (old.Address != updateCustomer.Address)
                    sb.AppendLine($"Adres: {old.Address} -> {updateCustomer.Address}");

                if (old.DeliveryAddress != updateCustomer.DeliveryAddress)
                    sb.AppendLine($"Teslimat Adresi: {old.DeliveryAddress} -> {updateCustomer.DeliveryAddress}");

                if (old.Contact != updateCustomer.Contact)
                    sb.AppendLine($"İletişim: {old.Contact} -> {updateCustomer.Contact}");

                if (old.Email != updateCustomer.Email)
                    sb.AppendLine($"E-posta: {old.Email} -> {updateCustomer.Email}");

                if (old.PhoneNumber != updateCustomer.PhoneNumber)
                    sb.AppendLine($"Telefon Numarası: {old.PhoneNumber} -> {updateCustomer.PhoneNumber}");

                if (old.TaxNo != updateCustomer.TaxNo)
                    sb.AppendLine($"Vergi No: {old.TaxNo} -> {updateCustomer.TaxNo}");

                if (old.TaxOffice != updateCustomer.TaxOffice)
                    sb.AppendLine($"Vergi Dairesi: {old.TaxOffice} -> {updateCustomer.TaxOffice}");

                if (old.CountryId != updateCustomer.CountryId)
                {
                    // İki ülke de null değilse isimle karşılaştır
                    string oldCountryName = old.CountryName ?? $"ID: {old.CountryId}";
                    string newCountryName = updateCustomer.Country?.CountryName ?? $"ID: {updateCustomer.CountryId}";

                    sb.AppendLine($"Ülke: {oldCountryName} -> {newCountryName}");
                }
                LogHelper.LogAction(_currentUser.Id, sb.ToString().Trim());

                Window.GetWindow(this)?.Close();
            }
            else
            {
                MessageBox.Show("Müşteri güncellenirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} değişiklik yapmadı. Güncelleme sayfasından çıkış yapıldı.");
            Window.GetWindow(this)?.Close();
        }

        private void cmbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }
}
