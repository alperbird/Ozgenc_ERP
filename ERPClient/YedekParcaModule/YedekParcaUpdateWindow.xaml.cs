using EntityLayer.Entities;
using ERPClient.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using User = EntityLayer.Entities.User;

namespace ERPClient.YedekParcaModule
{
    /// <summary>
    /// Interaction logic for YedekParcaUpdateWindow.xaml
    /// </summary>
    public partial class YedekParcaUpdateWindow : Window
    {
        private readonly User _currentUser;
        private Model.Product selectedProduct;
        private HttpClient client;
        private object user;

        public YedekParcaUpdateWindow(Model.Product selectedProduct, HttpClient client, User user)
        {
            InitializeComponent();
            _currentUser = user;
            contentFrame.Content = new YedekParcaUpdate(selectedProduct, client, _currentUser);
        }
    }
}
