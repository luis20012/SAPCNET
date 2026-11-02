using JuninComercial.Desktop.Models;

namespace JuninComercial.Desktop.Tests.Models;

/// <summary>
/// Tests de <see cref="Country"/>. Es una clase de datos sin lógica de negocio: lo que se
/// verifica es que los valores por defecto sean seguros para los bindings de WPF
/// (Name y Code nunca deben ser null) y que todos los campos se puedan asignar.
/// </summary>
public class CountryTests
{
    [Fact]
    public void Country_NuevoTieneNombreYCodigoVaciosNoNulos()
    {
        // Inicializados en string.Empty: los bindings de WPF no deben ver null.
        var c = new Country();

        Assert.Equal(string.Empty, c.Name);
        Assert.Equal(string.Empty, c.Code);
    }

    [Fact]
    public void Country_NuevoNoTieneIdNiAuditoria()
    {
        var c = new Country();

        Assert.Equal(0, c.IdCountry);
        Assert.Null(c.IdUserCreate);
        Assert.Null(c.IdUserUpdate);
        Assert.Null(c.DateCreate);
        Assert.Null(c.DateUpdate);
    }

    [Fact]
    public void Country_PermiteAsignarTodosLosCampos()
    {
        var fecha = new DateTime(2026, 1, 2, 3, 4, 5);

        var c = new Country
        {
            IdCountry = 7,
            Name = "Argentina",
            Code = "AR",
            IdUserCreate = 1,
            IdUserUpdate = 2,
            DateCreate = fecha,
            DateUpdate = fecha,
        };

        Assert.Equal(7, c.IdCountry);
        Assert.Equal("Argentina", c.Name);
        Assert.Equal("AR", c.Code);
        Assert.Equal(1, c.IdUserCreate);
        Assert.Equal(2, c.IdUserUpdate);
        Assert.Equal(fecha, c.DateCreate);
        Assert.Equal(fecha, c.DateUpdate);
    }
}
