using System.Collections.ObjectModel;
using System.Windows;
using JuninComercial.Desktop.Data;
using JuninComercial.Desktop.Models;

namespace JuninComercial.Desktop.Views;

/// <summary>
/// Equivalente a org.junincomercial.controller.CountryController.
/// </summary>
public partial class CountriesWindow : Window
{
    private const string ErrorConexion = "Verificá la conexión con la base de datos e intentá de nuevo.";

    private readonly ObservableCollection<Country> _data = new();
    private readonly CountryRepository _repo = new();

    public CountriesWindow()
    {
        InitializeComponent();
        Table.ItemsSource = _data;
        LoadData();
    }

    private void LoadData()
    {
        _data.Clear();
        foreach (var c in _repo.ListarTodas())
        {
            _data.Add(c);
        }
    }

    private void BtnNuevo_Click(object sender, RoutedEventArgs e)
    {
        OpenForm(null);
    }

    private void BtnEditar_Click(object sender, RoutedEventArgs e)
    {
        if (Table.SelectedItem is not Country selected)
        {
            ShowAlert("Seleccioná un país para editar.");
            return;
        }

        OpenForm(selected);
    }

    private void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (Table.SelectedItem is not Country selected)
        {
            ShowAlert("Seleccioná un país para eliminar.");
            return;
        }

        var existe = _repo.Existe(selected.IdCountry);
        var provincias = _repo.ContarProvincias(selected.IdCountry);

        if (existe == null || provincias == null)
        {
            ShowError($"No se pudo verificar el país. {ErrorConexion}");
            return;
        }

        if (existe == false)
        {
            ShowAlert($"\"{selected.Name}\" ya no existe (puede que otro usuario lo haya eliminado).");
            LoadData();
            return;
        }

        if (provincias > 0)
        {
            ShowAlert($"No se puede eliminar \"{selected.Name}\": tiene {provincias} provincia(s) asociada(s).");
            return;
        }

        var result = MessageBox.Show($"¿Eliminar \"{selected.Name}\"?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        if (!_repo.Eliminar(selected.IdCountry))
        {
            ShowError($"No se pudo eliminar el país. {ErrorConexion}");
            return;
        }

        LoadData();
    }

    private void BtnVolver_Click(object sender, RoutedEventArgs e)
    {
        var main = new MainWindow();
        main.Show();
        Close();
    }

    private void OpenForm(Country? country)
    {
        var form = new CountryFormWindow(country)
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        form.Saved += LoadData;
        form.ShowDialog();
    }

    private static void ShowAlert(string msg)
    {
        MessageBox.Show(msg, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private static void ShowError(string msg)
    {
        MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
