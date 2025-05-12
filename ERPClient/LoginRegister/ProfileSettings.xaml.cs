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
using Newtonsoft.Json;

namespace ERPClient.LoginRegister
{
    /// <summary>
    /// Interaction logic for ProfileSettings.xaml
    /// </summary>
    public partial class ProfileSettings : Window
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly User _currentUser;
        public ProfileSettings(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadUserInfo();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Profil ayarları sayfasından çıkış yaptı.");
            this.Close();
        }

        private async void LoadUserInfo()
        {
            txtUsername.Text = _currentUser.UserName;
            txtName.Text = _currentUser.Name;
            txtSurname.Text = _currentUser.Surname;

            try
            {
                var response = await _client.GetAsync("http://192.168.5.81:5001/api/Role");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var roles = JsonConvert.DeserializeObject<List<Role>>(json);

                    cmbDepartment.ItemsSource = roles;
                    cmbDepartment.DisplayMemberPath = "Department";
                    cmbDepartment.SelectedValuePath = "Id";
                    cmbDepartment.SelectedValue = _currentUser.RoleId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Departmanlar yüklenemedi: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = txtNewPassword.Password;
            string repeatPassword = txtNewPasswordRepeat.Password;

            if (!string.IsNullOrWhiteSpace(newPassword) || !string.IsNullOrWhiteSpace(repeatPassword))
            {
                if (newPassword != repeatPassword)
                {
                    MessageBox.Show("Yeni şifreler uyuşmuyor!", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var updateDto = new UserUpdateDto
            {
                Id = _currentUser.Id,
                UserName = txtUsername.Text.Trim(),
                Password = string.IsNullOrWhiteSpace(newPassword) ? _currentUser.Password : newPassword,
                RoleId = _currentUser.RoleId,
                Name = txtName.Text.Trim(),
                Surname = txtSurname.Text.Trim()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"http://192.168.5.81:5001/api/User/{_currentUser.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Bilgiler güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                _currentUser.UserName = updateDto.UserName.Trim();
                LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} profil bilgilerini güncelledi.");
                this.Close();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Hata oluştu: {response.StatusCode}\n{error}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
