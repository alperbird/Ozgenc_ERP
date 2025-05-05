using EntityLayer.Entities;
using ERPClient.FormModule;
using ERPClient.LoginRegister;
using ERPClient.Model;
using ERPClient.MusteriModule;
using ERPClient.SiparisModule;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace ERPClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly User _currentUser;
    public MainWindow(User user)
    {
        InitializeComponent();
        _currentUser = user;
    }

    private void btnMenuToggle_Click(object sender, RoutedEventArgs e)
    {
        MenuPanel.Visibility = MenuPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private void btnExit_Click(object sender, RoutedEventArgs e)
    {
        LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} sistemden çıkış yaptı.");
        AuthWindow login = new AuthWindow();
        login.Show();
        this.Close();
    }

    private void btnToggleTree_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string treeName)
        {
            var treeView = FindName(treeName) as TreeView;
            if (treeView != null)
            {
                treeView.Visibility = treeView.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
    private void TextBlock_Click(object sender, MouseButtonEventArgs e)
    {
        MessageBox.Show("Yedek Parça Listesi Açılıyor...");
        e.Handled = true;

        LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Yedek Parça sayfasına girdi.");
        YedekParca yp = new YedekParca(_currentUser);
        yp.Show();
    }

    private void TextBlockMusteri_Click(object sender, MouseButtonEventArgs e)
    {
        MessageBox.Show("Müşteri Listesi Açılıyor...");
        e.Handled = true;

        LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Müşteri sayfasına girdi.");
        CustomerList c = new CustomerList(_currentUser);
        c.Show();
    }

    private void Yurtİci_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Yurt İçi Sipariş sayfasına girdi.");
        YurtIci yi = new YurtIci(_currentUser);
        yi.Show();
    }

    private void YurtDisi_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Yurt Dışı Sipariş sayfasına girdi.");
        YurtDisi yd = new YurtDisi(_currentUser);
        yd.Show();
    }

    private void SiparisGecmisi_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Sipariş Geçmişi sayfasına girdi.");
        SiparisGecmisi sg = new SiparisGecmisi(_currentUser);
        sg.Show();
    }

    private void TeklifFormu_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Teklif Formu sayfasına girdi.");
        TeklifForm tf = new TeklifForm(_currentUser);
        tf.Show();
    }

    private void IsEmriFormu_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Malzeme İstek sayfasına girdi.");
        IsEmri ie = new IsEmri(_currentUser);
        ie.Show();
    }

}