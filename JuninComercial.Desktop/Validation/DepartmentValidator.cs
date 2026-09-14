using System.Text.RegularExpressions;

namespace JuninComercial.Desktop.Validation;

/// <summary>
/// Reglas de validación de formato para Department. El controller Java original no validaba
/// nada (Integer.parseInt con el campo vacío tiraba excepción); acá se valida antes de guardar.
/// Límites tomados de la tabla: name VARCHAR(45) NOT NULL, zipcode INT NULL, id_province INT NULL.
/// No valida unicidad ni existencia de la provincia: eso requiere acceso a la base y se
/// resuelve en DepartmentRepository, fuera de esta clase.
/// </summary>
public static partial class DepartmentValidator
{
    public const int NombreMinLength = 2;
    public const int NombreMaxLength = 45;

    // Códigos postales argentinos clásicos: 4 dígitos (Junín = 6000).
    public const int ZipcodeMin = 1000;
    public const int ZipcodeMax = 9999;

    // Letras (con acentos), dígitos ("9 de Julio", "25 de Mayo"), espacios y . ' -
    [GeneratedRegex(@"^[\p{L}0-9 .'\-]+$")]
    private static partial Regex NombreRegex();

    [GeneratedRegex(@"\p{L}")]
    private static partial Regex ContieneLetraRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspaciosRegex();

    /// <param name="Nombre">Nombre normalizado (sin espacios de más), listo para guardar.</param>
    public record Result(List<string> Errors, string Nombre, int? Zipcode, int? IdProvince);

    /// <param name="idProvince">ID elegido en el desplegable (null si no se eligió ninguna).</param>
    public static Result Validate(string? nombre, string? zipcode, int? idProvince)
    {
        var errors = new List<string>();

        nombre = EspaciosRegex().Replace(nombre?.Trim() ?? string.Empty, " ");
        zipcode = zipcode?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(nombre))
        {
            errors.Add("El nombre es obligatorio.");
        }
        else if (nombre.Length < NombreMinLength || nombre.Length > NombreMaxLength)
        {
            errors.Add($"El nombre debe tener entre {NombreMinLength} y {NombreMaxLength} caracteres.");
        }
        else if (!NombreRegex().IsMatch(nombre) || !ContieneLetraRegex().IsMatch(nombre))
        {
            errors.Add("El nombre solo puede tener letras, números, espacios y los signos . ' - (y al menos una letra).");
        }

        int? zip = null;
        if (string.IsNullOrEmpty(zipcode))
        {
            errors.Add("El código postal es obligatorio.");
        }
        else if (!int.TryParse(zipcode, out var z) || zipcode.Length != 4 || z < ZipcodeMin || z > ZipcodeMax)
        {
            errors.Add($"El código postal debe tener 4 dígitos ({ZipcodeMin}-{ZipcodeMax}).");
        }
        else
        {
            zip = z;
        }

        if (idProvince is null or <= 0)
        {
            errors.Add("Seleccioná una provincia.");
            idProvince = null;
        }

        return new Result(errors, nombre, zip, idProvince);
    }
}
