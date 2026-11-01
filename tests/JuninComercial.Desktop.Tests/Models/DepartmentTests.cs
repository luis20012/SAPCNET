using JuninComercial.Desktop.Models;

namespace JuninComercial.Desktop.Tests.Models;

/// <summary>
/// Tests de <see cref="Department"/>. Lo único con lógica es ProvinciaDisplay,
/// que es lo que termina viendo el usuario en la columna "Provincia" de la tabla.
/// </summary>
public class DepartmentTests
{
    [Fact]
    public void ProvinciaDisplay_ConNombreDeProvincia_MuestraElNombre()
    {
        var d = new Department { IdProvince = 7, ProvinceName = "Buenos Aires" };

        Assert.Equal("Buenos Aires", d.ProvinciaDisplay);
    }

    [Fact]
    public void ProvinciaDisplay_SinNombrePeroConId_MuestraElId()
    {
        // El LEFT JOIN no encontró la provincia: se muestra el id para no perder el dato.
        var d = new Department { IdProvince = 7, ProvinceName = null };

        Assert.Equal("7", d.ProvinciaDisplay);
    }

    [Fact]
    public void ProvinciaDisplay_SinNombreNiId_DevuelveCadenaVacia()
    {
        var d = new Department { IdProvince = null, ProvinceName = null };

        Assert.Equal(string.Empty, d.ProvinciaDisplay);
    }

    [Fact]
    public void ProvinciaDisplay_ConNombreYSinId_PrefiereElNombre()
    {
        var d = new Department { IdProvince = null, ProvinceName = "Buenos Aires" };

        Assert.Equal("Buenos Aires", d.ProvinciaDisplay);
    }

    [Fact]
    public void Department_NuevoTieneNombreVacioNoNulo()
    {
        // Name está inicializado en string.Empty: los bindings de WPF no deben ver null.
        var d = new Department();

        Assert.Equal(string.Empty, d.Name);
        Assert.Null(d.Zipcode);
        Assert.Null(d.IdProvince);
    }
}
