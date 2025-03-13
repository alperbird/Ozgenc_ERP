using ERPClient.MusteriModule;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ERPClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void btnMenuToggle_Click(object sender, RoutedEventArgs e)
    {
        MenuPanel.Visibility = MenuPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private void btnExit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
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

        YedekParca yp = new YedekParca();
        yp.Show();
    }

    private void TextBlockMusteri_Click(object sender, MouseButtonEventArgs e)
    {
        MessageBox.Show("Müşteri Listesi Açılıyor...");
        e.Handled = true;

        Customer c = new Customer();
        c.Show();
    }
}