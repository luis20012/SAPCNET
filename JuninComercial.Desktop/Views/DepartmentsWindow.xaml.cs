using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JuninComercial.Desktop.Data;
using JuninComercial.Desktop.Models;
using JuninComercial.Desktop.Validation;

namespace JuninComercial.Desktop.Views;

/// <summary>
/// Equivalente a org.junincomercial.controller.DepartmentController.
/// Misma pantalla única: formulario arriba, botones Crear/Modificar/Eliminar/Limpiar y tabla abajo.
/// La provincia se elige de un desplegable (muestra el nombre, guarda el id_province).
/// </summary>
public partial class DepartmentsWindow : Window
{
    private const string ErrorConexion = "Verificá la conexión con la base de datos e intentá de nuevo.";

    private readonly ObservableCollection<Department> _data = new();
    private readonly DepartmentRepository _repo = new();
    private readonly ProvinceRepository _provinceRepo = new();

    private Department? _seleccionado;

    public DepartmentsWindow()
    {
        InitializeComponent();
        TableDepartment.ItemsSource = _data;
        CargarProvincias();
        CargarTabla();
    }

    private void CargarProvincias()
    {
        var provincias = _provinceRepo.ListarTodas();
        CmbProvince.ItemsSource = provincias;

        if (provincias.Count == 0)
        {
            ShowAlert("No hay provincias cargadas (o no se pudo conectar a la base). " +
                      "Sin provincias no se pueden crear departamentos.");
        }
    }

    private void CargarTabla()
    {
        _data.Clear();
        foreach (var d in _repo.ListarTodos())
        {
            _data.Add(d);
        }
    }

    private void TableDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TableDepartment.SelectedItem is not Department d)
            return;

        _seleccionado = d;
        TxtName.Text = d.Name;
        TxtZipcode.Text = d.Zipcode?.ToString() ?? string.Empty;
        // Si el id_province guardado no está en el desplegable, queda sin selección
        // y hay que elegir una provincia válida para poder modificar.
        CmbProvince.SelectedValue = d.IdProvince;
    }

    private void BtnCrear_Click(object sender, RoutedEventArgs e)
    {
        if (!TryLeerFormulario(excludeId: null, out var datos))
            return;

        if (!_repo.Insertar(datos))
        {
            ShowError($"No se pudo crear el departamento. {ErrorConexion}");
            return;
        }

        CargarTabla();
        Limpiar();
    }

    private void BtnModificar_Click(object sender, RoutedEventArgs e)
    {
        if (_seleccionado == null)
        {
            ShowAlert("Seleccioná un departamento para modificar.");
            return;
        }

        if (!TryLeerFormulario(_seleccionado.IdDepartment, out var datos))
            return;

        if (datos.Name == _seleccionado.Name
            && datos.Zipcode == _seleccionado.Zipcode
            && datos.IdProvince == _seleccionado.IdProvince)
        {
            ShowAlert("No hay cambios para guardar.");
            return;
        }

        datos.IdDepartment = _seleccionado.IdDepartment;

        if (!_repo.Actualizar(datos))
        {
            ShowError($"No se pudo modificar el departamento. {ErrorConexion}");
            return;
        }

        CargarTabla();
        Limpiar();
    }

    private void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (_seleccionado == null)
        {
            ShowAlert("Seleccioná un departamento para eliminar.");
            return;
        }

        var distritos = _repo.ContarDistritos(_seleccionado.IdDepartment);
        if (distritos == null)
        {
            ShowError($"No se pudo verificar si el departamento tiene distritos. {ErrorConexion}");
            return;
        }

        if (distritos > 0)
        {
            ShowAlert($"No se puede eliminar \"{_seleccionado.Name}\": tiene {distritos} distrito(s) asociado(s).");
            return;
        }

        var result = MessageBox.Show($"¿Eliminar \"{_seleccionado.Name}\"?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        if (!_repo.Eliminar(_seleccionado.IdDepartment))
        {
            ShowError($"No se pudo eliminar el departamento. {ErrorConexion}");
            return;
        }

        CargarTabla();
        Limpiar();
    }

    private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
    {
        Limpiar();
    }

    private void BtnVolver_Click(object sender, RoutedEventArgs e)
    {
        var main = new MainWindow();
        main.Show();
        Close();
    }

    /// <summary>
    /// Valida formato y, si es correcto, las reglas que requieren la base:
    /// código postal único, nombre único dentro de la provincia y provincia existente.
    /// </summary>
    /// <param name="excludeId">Id del registro que se está modificando (null al crear).</param>
    private bool TryLeerFormulario(int? excludeId, out Department datos)
    {
        var idProvince = CmbProvince.SelectedValue as int?;
        var result = DepartmentValidator.Validate(TxtName.Text, TxtZipcode.Text, idProvince);
        var errores = result.Errors;

        datos = new Department
        {
            Name = result.Nombre,
            Zipcode = result.Zipcode,
            IdProvince = result.IdProvince,
        };

        // Solo se consulta la base si el formato ya es válido.
        if (errores.Count == 0)
        {
            // La provincia sale del desplegable, pero se re-verifica por si la borraron
            // mientras la ventana estaba abierta.
            var provinciaExiste = _repo.ExisteProvincia(datos.IdProvince!.Value);
            var zipRepetido = _repo.ExisteCodigoPostal(datos.Zipcode!.Value, excludeId);
            var nombreRepetido = _repo.ExisteNombreEnProvincia(datos.Name, datos.IdProvince, excludeId);

            if (provinciaExiste == null || zipRepetido == null || nombreRepetido == null)
            {
                ShowError($"No se pudieron verificar los datos. {ErrorConexion}");
                return false;
            }

            if (provinciaExiste == false)
                errores.Add("La provincia seleccionada ya no existe. Volvé a abrir la ventana.");

            if (zipRepetido == true)
                errores.Add($"Ya existe un departamento con el código postal {datos.Zipcode}.");

            if (nombreRepetido == true)
                errores.Add($"Ya existe un departamento llamado \"{datos.Name}\" en esa provincia.");
        }

        if (errores.Count > 0)
        {
            MessageBox.Show(string.Join(Environment.NewLine, errores), "Revisá los datos",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private void SoloDigitos_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsAsciiDigit);
    }

    private void SoloDigitos_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetData(typeof(string)) is not string texto || !texto.Trim().All(char.IsAsciiDigit))
        {
            e.CancelCommand();
        }
    }

    private void Limpiar()
    {
        TxtName.Clear();
        TxtZipcode.Clear();
        CmbProvince.SelectedIndex = -1;
        _seleccionado = null;
        TableDepartment.SelectedItem = null;
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
