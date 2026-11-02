using JuninComercial.Desktop.Validation;

namespace JuninComercial.Desktop.Tests.Validation;

/// <summary>
/// Tests de <see cref="CountryValidator"/>: la única pieza del módulo Country con reglas
/// puras (sin base de datos ni WPF), así que se puede testear directo.
/// La unicidad del código vive en CountryRepository.ExisteCodigo y necesita MySQL, por eso
/// no se cubre acá.
/// </summary>
public class CountryValidatorTests
{
    private const string NombreValido = "Argentina";
    private const string CodigoValido = "AR";

    // ---------------------------------------------------------------- caso feliz

    [Fact]
    public void ValidateFormat_ConDatosValidos_NoDevuelveErrores()
    {
        var errors = CountryValidator.ValidateFormat(NombreValido, CodigoValido);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("Perú")]                     // acentos
    [InlineData("Côte d'Ivoire")]            // apóstrofo
    [InlineData("Guinea-Bisáu")]             // guion
    [InlineData("St. Lucia")]                // punto
    [InlineData("Estados Unidos")]           // espacios internos
    [InlineData("Emiratos Árabes Unidos")]
    public void ValidateFormat_NombresReales_NoDevuelvenErrores(string nombre)
    {
        var errors = CountryValidator.ValidateFormat(nombre, CodigoValido);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("AR")]   // Argentina
    [InlineData("UY")]   // Uruguay
    [InlineData("BR")]   // Brasil
    [InlineData("US")]   // Estados Unidos
    public void ValidateFormat_CodigosIso2_NoDevuelvenErrores(string codigo)
    {
        var errors = CountryValidator.ValidateFormat(NombreValido, codigo);

        Assert.Empty(errors);
    }

    // ---------------------------------------------------------------- normalización

    [Fact]
    public void ValidateFormat_RecortaEspaciosDelNombre()
    {
        // El largo se mide después del Trim: "  Argentina  " son 9 caracteres, no 13.
        var errors = CountryValidator.ValidateFormat("  Argentina  ", CodigoValido);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateFormat_RecortaEspaciosDelCodigo()
    {
        var errors = CountryValidator.ValidateFormat(NombreValido, "  AR  ");

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("ar")]
    [InlineData("aR")]
    [InlineData("Ar")]
    public void ValidateFormat_CodigoEnMinusculas_SeAceptaPorqueSePasaAMayusculas(string codigo)
    {
        // El validador hace ToUpperInvariant antes de comparar contra ^[A-Z]{2}$.
        var errors = CountryValidator.ValidateFormat(NombreValido, codigo);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateFormat_NombreConEspaciosInternosDeMas_EsValido()
    {
        // A diferencia del validador de Department, este no colapsa espacios internos:
        // solo recorta los extremos, así que el largo sigue estando en rango.
        var errors = CountryValidator.ValidateFormat("Costa   Rica", CodigoValido);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(" A ")]       // queda "A" tras el recorte: 1 carácter
    [InlineData(" A\t ")]     // idem, con tab
    public void ValidateFormat_NombreCortoConEspaciosAlrededor_DevuelveErrorDeLongitud(string nombre)
    {
        var errors = CountryValidator.ValidateFormat(nombre, CodigoValido);

        Assert.Contains(errors, e => e.Contains("entre 2 y 100 caracteres"));
    }

    // ---------------------------------------------------------------- nombre inválido

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void ValidateFormat_SinNombre_DevuelveErrorDeObligatorio(string? nombre)
    {
        var errors = CountryValidator.ValidateFormat(nombre, CodigoValido);

        Assert.Contains("El nombre es obligatorio.", errors);
    }

    [Fact]
    public void ValidateFormat_NombreDemasiadoCorto_DevuelveErrorDeLongitud()
    {
        var errors = CountryValidator.ValidateFormat("A", CodigoValido);

        Assert.Contains(errors, e => e.Contains("entre 2 y 100 caracteres"));
    }

    [Fact]
    public void ValidateFormat_NombreDemasiadoLargo_DevuelveErrorDeLongitud()
    {
        var nombre = new string('A', CountryValidator.NombreMaxLength + 1);

        var errors = CountryValidator.ValidateFormat(nombre, CodigoValido);

        Assert.Contains(errors, e => e.Contains("entre 2 y 100 caracteres"));
    }

    [Theory]
    [InlineData(CountryValidator.NombreMinLength)]   // 2: límite inferior exacto
    [InlineData(CountryValidator.NombreMaxLength)]   // 100: límite superior exacto
    public void ValidateFormat_NombreEnElLimiteDeLongitud_EsValido(int largo)
    {
        var nombre = new string('A', largo);

        var errors = CountryValidator.ValidateFormat(nombre, CodigoValido);

        Assert.Empty(errors);
    }

    // ---------------------------------------------------------------- código inválido

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateFormat_SinCodigo_DevuelveErrorDeObligatorio(string? codigo)
    {
        var errors = CountryValidator.ValidateFormat(NombreValido, codigo);

        Assert.Contains("El código es obligatorio.", errors);
    }

    [Theory]
    [InlineData("A")]           // 1 letra
    [InlineData("ARG")]         // 3 letras: no es ISO 3166-1 alpha-2
    [InlineData("ARGENTINA")]   // el nombre completo pegado en el campo código
    [InlineData("12")]          // solo dígitos
    [InlineData("A1")]          // letra + dígito
    [InlineData("A R")]         // espacio interno
    [InlineData("AR!")]         // símbolo
    [InlineData("ÑU")]          // Ñ no está en el rango [A-Z]
    public void ValidateFormat_CodigoInvalido_DevuelveError(string codigo)
    {
        var errors = CountryValidator.ValidateFormat(NombreValido, codigo);

        Assert.Contains(errors, e => e.Contains("exactamente 2 letras"));
    }

    // ---------------------------------------------------------------- errores combinados

    [Fact]
    public void ValidateFormat_ConTodosLosCamposVacios_AcumulaLosDosErrores()
    {
        var errors = CountryValidator.ValidateFormat(null, null);

        Assert.Equal(2, errors.Count);
        Assert.Contains("El nombre es obligatorio.", errors);
        Assert.Contains("El código es obligatorio.", errors);
    }

    [Fact]
    public void ValidateFormat_NombreInvalido_ReportaUnSoloErrorDeNombre()
    {
        // Las reglas de nombre son excluyentes (if / else if): nunca se apilan dos.
        var errors = CountryValidator.ValidateFormat("A", CodigoValido);

        Assert.Single(errors);
    }

    [Fact]
    public void ValidateFormat_CodigoInvalido_ReportaUnSoloErrorDeCodigo()
    {
        // Idem para el código: obligatorio y formato son excluyentes.
        var errors = CountryValidator.ValidateFormat(NombreValido, "A");

        Assert.Single(errors);
    }
}
