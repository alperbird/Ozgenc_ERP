using DataAccessLayer.Data;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataAccessLayer.Data;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;


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

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
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
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer("Server=192.168.5.81,1433;Database=Ozgenc_ERP;User Id=sa;Password=ozgencmachine16;TrustServerCertificate=True;")
                    .Options;
                using (var context = new AppDbContext(options)) // ← kendi context adını yaz knk
                {
                    var user = context.Users
                        .Include(u => u.Role)
                        .FirstOrDefault(u => u.UserName == username && u.Password == password);

                    if (user == null)
                    {
                        MessageBox.Show("Kullanıcı adı veya şifre hatalı.", "Hatalı Giriş", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Log Kaydı
                    var log = new Log
                    {
                        UserId = user.Id,
                        Information = $"{user.Name} {user.Surname} giriş yaptı",
                        Date = DateTime.Now
                    };

                    context.Logs.Add(log);
                    context.SaveChanges();

                    // Ana pencereyi aç
                    MainWindow main = new MainWindow(user); // ← ana ekran hangisiyse onu aç
                    main.Show();

                    // Giriş ekranını kapat
                    Window.GetWindow(this)?.Close();
                }
            }
            catch (Exception)
            {

                throw;
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
