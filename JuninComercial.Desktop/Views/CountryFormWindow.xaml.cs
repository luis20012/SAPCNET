using System.Windows;
using JuninComercial.Desktop.Data;
using JuninComercial.Desktop.Models;
using JuninComercial.Desktop.Validation;

namespace JuninComercial.Desktop.Views;

/// <summary>
/// Equivalente a org.junincomercial.controller.CountryFormController.
/// </summary>
public partial class CountryFormWindow : Window
{
    private readonly Country? _country;
    private readonly CountryRepository _repo = new();

    /// <summary>Equivalente al Runnable onSave del controller Java.</summary>
    public event Action? Saved;

    public CountryFormWindow(Country? country)
    {
        InitializeComponent();
        _country = country;

        Title = country == null ? "Nuevo País" : "Editar País";

        if (country != null)
        {
            TxtNombre.Text = country.Name;
            TxtCodigo.Text = country.Code;
        }
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        var nombre = TxtNombre.Text.Trim();
        var codigo = TxtCodigo.Text.Trim().ToUpperInvariant();

        var errores = CountryValidator.ValidateFormat(nombre, codigo);

        // La unicidad de código solo se chequea si el formato ya es válido
        // (no tiene sentido pegarle a la base con un código con formato incorrecto).
        if (errores.Count == 0 && _repo.ExisteCodigo(codigo, _country?.IdCountry))
        {
            errores.Add($"Ya existe un país con el código \"{codigo}\".");
        }

        if (errores.Count > 0)
        {
            MessageBox.Show(string.Join(Environment.NewLine, errores), "Revisá los datos",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        bool ok;
        if (_country == null)
        {
            ok = _repo.Insertar(new Country { Name = nombre, Code = codigo });
        }
        else
        {
            _country.Name = nombre;
            _country.Code = codigo;
            ok = _repo.Actualizar(_country);
        }

        if (!ok)
        {
            MessageBox.Show(
                "No se pudo guardar el país. Verificá la conexión con la base de datos e intentá de nuevo.",
                "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Saved?.Invoke();
        Close();
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
