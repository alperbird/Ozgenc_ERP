using ERPClient.Model;
using Newtonsoft.Json;
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

namespace ERPClient.MusteriModule
{
    /// <summary>
    /// Interaction logic for CountryCRUDWindow.xaml
    /// </summary>
    public partial class CountryCRUDWindow : Window
    {
        private readonly HttpClient _client;
        private const string apiUrl = "http://192.168.5.81:5001/api/Country";
        private List<Country> _countries;
        private Country? _selectedCountry = null;

        public CountryCRUDWindow(HttpClient client)
        {
            InitializeComponent();
            _client = client;
            LoadCountries();
        }

        private async void LoadCountries()
        {
            HttpResponseMessage response = await _client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            _countries = JsonConvert.DeserializeObject<List<Country>>(json) ?? new();
            lstCountries.ItemsSource = null;
            lstCountries.ItemsSource = _countries;
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string newCountryName = txtCountryName.Text.Trim();
            if (string.IsNullOrWhiteSpace(newCountryName))
            {
                MessageBox.Show("Ülke adı boş olamaz.");
                return;
            }

            var newCountry = new Country { CountryName = newCountryName };
            string json = JsonConvert.SerializeObject(newCountry);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                txtCountryName.Text = "";
                LoadCountries();
            }
            else
            {
                MessageBox.Show("Ülke eklenemedi.");
            }
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCountry == null)
            {
                MessageBox.Show("Lütfen güncellemek için bir ülke seçin!");
                return;
            }

            string updatedName = txtCountryName.Text.Trim();
            if (string.IsNullOrWhiteSpace(updatedName))
            {
                MessageBox.Show("Ülke adı boş olamaz.");
                return;
            }

            _selectedCountry.CountryName = updatedName;
            string json = JsonConvert.SerializeObject(_selectedCountry);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync($"{apiUrl}/{_selectedCountry.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Ülke güncellendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCountryName.Text = "";
                _selectedCountry = null;
                LoadCountries();
            }
            else
            {
                MessageBox.Show("Güncelleme başarısız oldu.");
            }
        }

        // SİLME
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCountry == null)
            {
                MessageBox.Show("Lütfen silmek için bir ülke seçin!");
                return;
            }

            var confirm = MessageBox.Show($"'{_selectedCountry.CountryName}' silinsin mi?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            HttpResponseMessage response = await _client.DeleteAsync($"{apiUrl}/{_selectedCountry.Id}");

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Ülke silindi!", "Silme", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCountryName.Text = "";
                _selectedCountry = null;
                LoadCountries();
            }
            else
            {
                MessageBox.Show("Ülke silinemedi.");
            }
        }

        private void lstCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCountry = lstCountries.SelectedItem as Country;
            if (_selectedCountry != null)
            {
                txtCountryName.Text = _selectedCountry.CountryName;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtCountryName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnAdd_Click(sender, e);
            }
        }
    }
}
