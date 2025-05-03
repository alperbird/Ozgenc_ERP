using DataAccessLayer.Data;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
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

            var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer("Server=192.168.5.81,1433;Database=Ozgenc_ERP;User Id=sa;Password=ozgencmachine16;TrustServerCertificate=True;")
                    .Options;
            using (var context = new AppDbContext(options))
            {
                // Aynı kullanıcı adı var mı kontrol
                if (context.Users.Any(u => u.UserName == username))
                {
                    MessageBox.Show("Bu kullanıcı adı zaten kullanılıyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newUser = new User
                {
                    Name = name,
                    Surname = surname,
                    UserName = username,
                    Password = password,
                    RoleId = selectedRole.Id
                };

                context.Users.Add(newUser);
                context.SaveChanges();

                var log = new Log
                {
                    UserId = newUser.Id,
                    Information = $"{newUser.Name} {newUser.Surname} adlı kullanıcı sisteme kayıt oldu.",
                    Date = DateTime.Now
                };

                context.Logs.Add(log);
                context.SaveChanges();
            }

            MessageBox.Show("Kayıt başarıyla oluşturuldu!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            _parent.ShowLogin(); // login ekranına dön
        }

        private void LoadRoles()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer("Server=192.168.5.81,1433;Database=Ozgenc_ERP;User Id=sa;Password=ozgencmachine16;TrustServerCertificate=True;")
                    .Options;
            using (var context = new AppDbContext(options))
            {
                var roles = context.Roles.ToList();
                cmbRoles.ItemsSource = roles;
            }
        }
    }
}
