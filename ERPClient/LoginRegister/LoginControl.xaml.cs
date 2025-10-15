using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net.Http;
using System.Text.Json;
using Microsoft.IdentityModel.Logging;
using ERPClient.Model;


namespace ERPClient.LoginRegister
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
        private AuthWindow _parent;
        public LoginControl(AuthWindow parent)
        {
            InitializeComponent();
            _parent = parent;
        }

        private void RegisterText_Click(object sender, MouseButtonEventArgs e)
        {
           _parent.ShowRegister();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Lütfen kullanıcı adı veya şifrenizi giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var client = new HttpClient();
                var loginData = new
                {
                    Username = username,
                    Password = password
                };

                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://192.168.5.81:5001/api/User/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<User>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (user != null)
                    {
                        MainWindow main = new MainWindow(user); // ← kullanıcıyı gönderiyoruz
                        Model.LogHelper.LogAction(user.Id, $"{user.Name} {user.Surname} sisteme giriş yaptı.");
                        main.Show();
                        Window.GetWindow(this)?.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Kullanıcı adı veya şifre hatalı.", "Hatalı Giriş", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Sunucuya bağlanılamadı! Lütfen daha sonra tekrar deneyiniz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Ivır Zıvırlar
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }
        #endregion
    }
}
