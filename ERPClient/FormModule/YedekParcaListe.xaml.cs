using DataAccessLayer.Data;
using DataAccessLayer.Repositories;
using EntityLayer.Entities;
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
using System.Windows.Shapes;

namespace ERPClient.FormModule
{
    /// <summary>
    /// Interaction logic for YedekParcaListe.xaml
    /// </summary>
    public partial class YedekParcaListe : Window
    {
        private readonly ProductRepository _repo;
        public List<string> SecilenParcalar { get; set; } = new List<string>();

        public YedekParcaListe(ProductRepository repo)
        { 
            _repo = repo;
            InitializeComponent();
            YedekParcalariYukle();
        }

        private async void YedekParcalariYukle()
        {
            try
            {
                var yp = await _repo.GetAllAsync(); // _repo'yu kullan

                foreach (var item in yp)
                {
                    lstParcalar.Items.Add(item.ParcaAdi); // sadece parça adı
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yedek Parça Listelenirken Hata: {ex.Message}");
            }
        }

        private void BtnTamam_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in lstParcalar.SelectedItems)
            {
                SecilenParcalar.Add(item.ToString());
            }

            this.DialogResult = true;
            this.Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
