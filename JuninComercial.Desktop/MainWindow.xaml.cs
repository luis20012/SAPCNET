using System.Windows;
using JuninComercial.Desktop.Views;

namespace JuninComercial.Desktop;

/// <summary>
/// Equivalente a org.junincomercial.PrimaryController.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MenuPaises_Click(object sender, RoutedEventArgs e)
    {
        var countriesWindow = new CountriesWindow
        {
            Owner = this
        };
        countriesWindow.Show();
        Close();
    }

    private void MenuCerrar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
