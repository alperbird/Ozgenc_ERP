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

        LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Yedek Parça sistemine girdi.");
        YedekParca yp = new YedekParca(_currentUser);
        yp.Show();
    }

    private void TextBlockMusteri_Click(object sender, MouseButtonEventArgs e)
    {
        MessageBox.Show("Müşteri Listesi Açılıyor...");
        e.Handled = true;

        CustomerList c = new CustomerList();
        c.Show();
    }

    private void Yurtİci_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        YurtIci yi = new YurtIci();
        yi.Show();
    }

    private void YurtDisi_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        YurtDisi yd = new YurtDisi();
        yd.Show();
    }

    private void SiparisGecmisi_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        SiparisGecmisi sg = new SiparisGecmisi();
        sg.Show();
    }

    private void TeklifFormu_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        TeklifForm tf = new TeklifForm();
        tf.Show();
    }

    private void IsEmriFormu_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        IsEmri ie = new IsEmri();
        ie.Show();
    }

}