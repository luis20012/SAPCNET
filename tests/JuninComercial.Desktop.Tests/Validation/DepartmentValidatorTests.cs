using JuninComercial.Desktop.Validation;

namespace JuninComercial.Desktop.Tests.Validation;

/// <summary>
/// Tests de <see cref="DepartmentValidator"/>: la única pieza del módulo Department con
/// reglas de negocio puras (sin base de datos ni WPF), así que se puede testear directo.
/// </summary>
public class DepartmentValidatorTests
{
    private const int ProvinciaValida = 1;
    private const string NombreValido = "Junín";
    private const string ZipValido = "6000";

    // ---------------------------------------------------------------- caso feliz

    [Fact]
    public void Validate_ConDatosValidos_NoDevuelveErrores()
    {
        var result = DepartmentValidator.Validate(NombreValido, ZipValido, ProvinciaValida);

        Assert.Empty(result.Errors);
        Assert.Equal("Junín", result.Nombre);
        Assert.Equal(6000, result.Zipcode);
        Assert.Equal(ProvinciaValida, result.IdProvince);
    }

    [Theory]
    [InlineData("Junín")]                 // acentos
    [InlineData("9 de Julio")]            // dígitos + espacios
    [InlineData("25 de Mayo")]
    [InlineData("General Viamonte")]
    [InlineData("O'Higgins")]             // apóstrofo
    [InlineData("Gral. Arenales")]        // punto
    [InlineData("Leandro N. Alem")]
    [InlineData("Tres-Lomas")]            // guion
    public void Validate_NombresConFormatoPermitido_NoDevuelveErrores(string nombre)
    {
        var result = DepartmentValidator.Validate(nombre, ZipValido, ProvinciaValida);

        Assert.Empty(result.Errors);
    }

    // ---------------------------------------------------------------- normalización

    [Theory]
    [InlineData("  Junín  ", "Junín")]                          // recorta extremos
    [InlineData("9   de    Julio", "9 de Julio")]               // colapsa espacios internos
    [InlineData("\tGeneral\n Viamonte ", "General Viamonte")]   // tabs y saltos de línea
    public void Validate_NormalizaEspaciosDelNombre(string entrada, string esperado)
    {
        var result = DepartmentValidator.Validate(entrada, ZipValido, ProvinciaValida);

        Assert.Empty(result.Errors);
        Assert.Equal(esperado, result.Nombre);
    }

    [Fact]
    public void Validate_RecortaEspaciosDelZipcode()
    {
        var result = DepartmentValidator.Validate(NombreValido, "  6000  ", ProvinciaValida);

        Assert.Empty(result.Errors);
        Assert.Equal(6000, result.Zipcode);
    }

    [Fact]
    public void Validate_NoCambiaMayusculasDelNombre()
    {
        // A diferencia del código de país, el nombre se guarda tal cual lo escribió el usuario.
        var result = DepartmentValidator.Validate("gEnErAl ViAmOnTe", ZipValido, ProvinciaValida);

        Assert.Empty(result.Errors);
        Assert.Equal("gEnErAl ViAmOnTe", result.Nombre);
    }

    // ---------------------------------------------------------------- nombre inválido

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Validate_SinNombre_DevuelveErrorDeObligatorio(string? nombre)
    {
        var result = DepartmentValidator.Validate(nombre, ZipValido, ProvinciaValida);

        Assert.Contains("El nombre es obligatorio.", result.Errors);
        Assert.Equal(string.Empty, result.Nombre);
    }

    [Fact]
    public void Validate_NombreDemasiadoCorto_DevuelveErrorDeLongitud()
    {
        var result = DepartmentValidator.Validate("J", ZipValido, ProvinciaValida);

        Assert.Contains(result.Errors, e => e.Contains("entre 2 y 45 caracteres"));
    }

    [Fact]
    public void Validate_NombreDemasiadoLargo_DevuelveErrorDeLongitud()
    {
        var nombre = new string('A', DepartmentValidator.NombreMaxLength + 1);

        var result = DepartmentValidator.Validate(nombre, ZipValido, ProvinciaValida);

        Assert.Contains(result.Errors, e => e.Contains("entre 2 y 45 caracteres"));
    }

    [Theory]
    [InlineData(DepartmentValidator.NombreMinLength)]   // 2: límite inferior exacto
    [InlineData(DepartmentValidator.NombreMaxLength)]   // 45: límite superior exacto
    public void Validate_NombreEnElLimiteDeLongitud_EsValido(int largo)
    {
        var nombre = new string('A', largo);

        var result = DepartmentValidator.Validate(nombre, ZipValido, ProvinciaValida);

        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("Junín!")]        // signo no permitido
    [InlineData("Junín@2024")]
    [InlineData("Junín_Norte")]   // guion bajo no está en la lista
    [InlineData("Junín/Oeste")]
    [InlineData("<script>")]
    [InlineData("1234")]          // solo dígitos: no tiene ninguna letra
    [InlineData("25 - 9")]        // dígitos y signos, sin letras
    public void Validate_NombreConCaracteresNoPermitidos_DevuelveError(string nombre)
    {
        var result = DepartmentValidator.Validate(nombre, ZipValido, ProvinciaValida);

        Assert.Contains(result.Errors, e => e.Contains("solo puede tener letras"));
    }

    // ---------------------------------------------------------------- zipcode inválido

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_SinZipcode_DevuelveErrorDeObligatorio(string? zipcode)
    {
        var result = DepartmentValidator.Validate(NombreValido, zipcode, ProvinciaValida);

        Assert.Contains("El código postal es obligatorio.", result.Errors);
        Assert.Null(result.Zipcode);
    }

    [Theory]
    [InlineData("600")]      // 3 dígitos
    [InlineData("60000")]    // 5 dígitos
    [InlineData("0600")]     // 4 caracteres pero < 1000 al parsear
    [InlineData("0000")]
    [InlineData("abcd")]     // no numérico
    [InlineData("60a0")]
    [InlineData("-600")]     // 4 caracteres, negativo
    [InlineData("6 00")]     // espacio interno
    [InlineData("6000.0")]
    public void Validate_ZipcodeInvalido_DevuelveErrorYNoLoDevuelve(string zipcode)
    {
        var result = DepartmentValidator.Validate(NombreValido, zipcode, ProvinciaValida);

        Assert.Contains(result.Errors, e => e.Contains("4 dígitos"));
        Assert.Null(result.Zipcode);
    }

    [Theory]
    [InlineData("1000", 1000)]   // límite inferior exacto
    [InlineData("9999", 9999)]   // límite superior exacto
    [InlineData("6000", 6000)]   // Junín
    public void Validate_ZipcodeEnElRango_EsValido(string zipcode, int esperado)
    {
        var result = DepartmentValidator.Validate(NombreValido, zipcode, ProvinciaValida);

        Assert.Empty(result.Errors);
        Assert.Equal(esperado, result.Zipcode);
    }

    // ---------------------------------------------------------------- provincia inválida

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_SinProvinciaValida_DevuelveErrorYNormalizaANull(int? idProvince)
    {
        var result = DepartmentValidator.Validate(NombreValido, ZipValido, idProvince);

        Assert.Contains("Seleccioná una provincia.", result.Errors);
        Assert.Null(result.IdProvince);
    }

    // ---------------------------------------------------------------- errores combinados

    [Fact]
    public void Validate_ConTodosLosCamposInvalidos_AcumulaLosTresErrores()
    {
        var result = DepartmentValidator.Validate(null, null, null);

        Assert.Equal(3, result.Errors.Count);
        Assert.Contains("El nombre es obligatorio.", result.Errors);
        Assert.Contains("El código postal es obligatorio.", result.Errors);
        Assert.Contains("Seleccioná una provincia.", result.Errors);
    }

    [Fact]
    public void Validate_NombreInvalido_ReportaUnSoloErrorDeNombre()
    {
        // Las reglas de nombre son excluyentes (if / else if): nunca se apilan dos.
        var result = DepartmentValidator.Validate("!", ZipValido, ProvinciaValida);

        Assert.Single(result.Errors);
    }
}
