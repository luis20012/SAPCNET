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
        // Sin Owner: en WPF, cerrar la ventana dueña cierra también las ventanas que posee,
        // y acá se cierra MainWindow justo después de abrir Países.
        var countriesWindow = new CountriesWindow();
        countriesWindow.Show();
        Close();
    }

    private void MenuDepartamentos_Click(object sender, RoutedEventArgs e)
    {
        // Sin Owner: en WPF, cerrar la ventana dueña cierra también las ventanas que posee.
        var departmentsWindow = new DepartmentsWindow();
        departmentsWindow.Show();
        Close();
    }

    private void MenuCerrar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
