using DataAccessLayer.Data;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ERPClient.LoginRegister
{
    /// <summary>
    /// Interaction logic for rEGİSTERControl.xaml
    /// </summary>
    public partial class RegisterControl : UserControl
    {
        private AuthWindow _parent;
        public RegisterControl(AuthWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            LoadRoles();
        }

        private void LoginText_Click(object sender, MouseButtonEventArgs e)
        {
            _parent.ShowLogin();
        }

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string surname = txtSurname.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;
            var selectedRole = cmbRoles.SelectedItem as Role;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname) ||
                string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || selectedRole == null)
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newUserDto = new
            {
                Name = name,
                Surname = surname,
                Username = username,
                Password = password,
                RoleId = selectedRole.Id
            };

            try
            {
                using var client = new HttpClient();
                var json = System.Text.Json.JsonSerializer.Serialize(newUserDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://192.168.5.81:5001/api/User/register", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Kayıt başarıyla oluşturuldu!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    _parent.ShowLogin(); // login ekranına dön
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Kayıt başarısız!\n{error}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("API bağlantı hatası:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void LoadRoles()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync("http://192.168.5.81:5001/api/Role");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var roles = JsonConvert.DeserializeObject<List<Role>>(json);
                    cmbRoles.ItemsSource = roles;
                }
                else
                {
                    MessageBox.Show("Roller yüklenemedi!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmedik hata: {ex.Message}");
            }
        }


    }
}
