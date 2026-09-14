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
        var result = CountryValidator.Validate(TxtNombre.Text, TxtCodigo.Text);
        var errores = result.Errors;
        var nombre = result.Nombre;
        var codigo = result.Codigo;
        var excludeId = _country?.IdCountry;

        // Al editar: si no se cambió nada, no hace falta ir a la base.
        // (Se compara exacto para permitir corregir mayúsculas/minúsculas del nombre.)
        if (errores.Count == 0 && _country != null && nombre == _country.Name && codigo == _country.Code)
        {
            MessageBox.Show("No hay cambios para guardar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Las reglas que requieren la base solo se chequean si el formato ya es válido.
        if (errores.Count == 0)
        {
            var existe = _country == null ? true : _repo.Existe(_country.IdCountry);
            var codigoRepetido = _repo.ExisteCodigo(codigo, excludeId);
            var nombreRepetido = _repo.ExisteNombre(nombre, excludeId);

            if (existe == null || codigoRepetido == null || nombreRepetido == null)
            {
                ShowError("No se pudieron verificar los datos. Verificá la conexión con la base de datos e intentá de nuevo.");
                return;
            }

            if (existe == false)
            {
                MessageBox.Show("El país que estás editando ya no existe (puede que otro usuario lo haya eliminado).",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                Saved?.Invoke(); // refresca la grilla
                Close();
                return;
            }

            if (codigoRepetido == true)
                errores.Add($"Ya existe un país con el código \"{codigo}\".");

            if (nombreRepetido == true)
                errores.Add($"Ya existe un país llamado \"{nombre}\".");
        }

        if (errores.Count > 0)
        {
            MessageBox.Show(string.Join(Environment.NewLine, errores), "Revisá los datos",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Se arma un objeto nuevo en vez de modificar _country: si la base falla,
        // la fila de la grilla no queda mostrando datos que no se guardaron.
        var datos = new Country { IdCountry = excludeId ?? 0, Name = nombre, Code = codigo };
        var ok = _country == null ? _repo.Insertar(datos) : _repo.Actualizar(datos);

        if (!ok)
        {
            ShowError("No se pudo guardar el país. Verificá la conexión con la base de datos e intentá de nuevo.");
            return;
        }

        Saved?.Invoke();
        Close();
    }

    private static void ShowError(string msg)
    {
        MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
