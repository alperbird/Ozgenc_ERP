using ClosedXML.Excel;
using DataAccessLayer.Data;
using DataAccessLayer.Repositories;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using DocumentFormat.OpenXml.Wordprocessing;
using ERPClient.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace ERPClient.FormModule
{
    /// <summary>
    /// Interaction logic for IsEmri.xaml
    /// </summary>
    public partial class IsEmri : Window
    {
        private readonly ProductRepository _repo;
        private ObservableCollection<MalzemeList> malzemeListesi = new ObservableCollection<MalzemeList>();

        public IsEmri()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer("Server=192.168.5.81,1433;Database=Ozgenc_ERP;User Id=sa;Password=ozgencmachine16;TrustServerCertificate=True;")
                    .Options;
            var context = new AppDbContext(options);
            _repo = new ProductRepository(context);
            InitializeComponent();
            //dpTarih.SelectedDate = DateTime.Now;
            dgMalzemeListesi.ItemsSource = malzemeListesi;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void BtnYazdir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "MALZEME İSTEK FORMU.xlsx");
                string outputPath = Path.Combine(Path.GetTempPath(), "MalzemeIstekFormu_Doldurulmus.xlsx");

                File.Copy(templatePath, outputPath, true); // Şablonu geçici klasöre kopyala

                using (var workbook = new XLWorkbook(outputPath))
                {
                    var worksheet = workbook.Worksheet(1); // 1. Sayfa

                    // ÜST BİLGİLERİ DOLDUR
                    worksheet.Cell("D6").Value = txtFirma.Text;
                    worksheet.Cell("D7").Value = dpTalepGelmeTarihi.SelectedDate?.ToString("dd.MM.yyyy");
                    worksheet.Cell("D8").Value = dpInvoiceAtilmaTarihi.SelectedDate?.ToString("dd.MM.yyyy");
                    worksheet.Cell("D9").Value = dpVerilenTermin.SelectedDate?.ToString("dd.MM.yyyy");
                    worksheet.Cell("D10").Value = txtOdemeDurum.Text;
                    worksheet.Cell("D11").Value = txtAciklama.Text;

                    // MALZEME LİSTESİ
                    int startRow = 13;
                    int no = 1;

                    foreach (var item in dgMalzemeListesi.Items)
                    {
                        if (item is not null && item is MalzemeList malzeme)
                        {
                            worksheet.Cell(startRow, "A").Value = no;                     // No
                            worksheet.Cell(startRow, "B").Value = malzeme.MalzemeAdi;     // Malzeme Adı
                            worksheet.Cell(startRow, "E").Value = malzeme.TalepMiktari;   // Talep Miktarı
                            worksheet.Cell(startRow, "F").Value = malzeme.TuketimZamani;  // Tüketim Zamanı
                            worksheet.Cell(startRow, "G").Value = malzeme.IhtiyacTarihi;  // İhtiyaç Tarihi
                            worksheet.Cell(startRow, "H").Value = malzeme.Not;            // Not
                            startRow++;
                            no++;
                        }
                    }

                    workbook.Save();
                }

                // Excel dosyasını açalım
                Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"HATA: {ex.Message}");
            }
        }


        private void BtnParcaEkle_Click(object sender, RoutedEventArgs e)
        {
            var parcaSec = new YedekParcaListe(_repo); // repo veriyoruz!

            if (parcaSec.ShowDialog() == true)
            {
                foreach (var parca in parcaSec.SecilenParcalar)
                {
                    malzemeListesi.Add(new MalzemeList
                    {
                        MalzemeAdi = parca,
                        TalepMiktari = 1,
                        TuketimZamani = "",
                        IhtiyacTarihi = "",
                        Not = ""
                    });
                }
            }
        }

        #region Çok salakça bir şey
        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (dgMalzemeListesi.SelectedItem is MalzemeList selectedItem)
            {
                malzemeListesi.Remove(selectedItem); // silme işte
            }
        }

        private void dgMalzemeListesi_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }
        #endregion

        
    }
}
