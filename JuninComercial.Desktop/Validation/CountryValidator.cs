using System.Text.RegularExpressions;

namespace JuninComercial.Desktop.Validation;

/// <summary>
/// Reglas de validación de formato para Country. Separado del code-behind de la
/// ventana para poder testearlo/reusarlo sin depender de WPF.
/// No valida unicidad ni existencia: eso requiere acceso a la base y se resuelve
/// en CountryRepository, fuera de esta clase.
/// </summary>
public static partial class CountryValidator
{
    public const int NombreMinLength = 2;
    public const int NombreMaxLength = 100;

    public const int CodigoLength = 4;

    // Exactamente 4 letras mayúsculas.
    [GeneratedRegex("^[A-Z]{4}$")]
    private static partial Regex CodigoRegex();

    // Letras (con acentos), espacios y . ' - ("Costa de Marfil", "Guinea-Bisáu", "St. Lucia").
    [GeneratedRegex(@"^[\p{L} .'\-]+$")]
    private static partial Regex NombreRegex();

    [GeneratedRegex(@"\p{L}")]
    private static partial Regex ContieneLetraRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspaciosRegex();

    /// <param name="Nombre">Nombre normalizado (sin espacios de más), listo para guardar.</param>
    /// <param name="Codigo">Código normalizado (sin espacios, en mayúsculas), listo para guardar.</param>
    public record Result(List<string> Errors, string Nombre, string Codigo);

    public static Result Validate(string? nombre, string? codigo)
    {
        var errors = new List<string>();

        nombre = EspaciosRegex().Replace(nombre?.Trim() ?? string.Empty, " ");
        codigo = codigo?.Trim().ToUpperInvariant() ?? string.Empty;

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
            errors.Add("El nombre solo puede tener letras, espacios y los signos . ' -");
        }

        if (string.IsNullOrEmpty(codigo))
        {
            errors.Add("El código es obligatorio.");
        }
        else if (!CodigoRegex().IsMatch(codigo))
        {
            errors.Add($"El código debe tener exactamente {CodigoLength} letras.");
        }

        return new Result(errors, nombre, codigo);
    }
}
