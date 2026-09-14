using System.Text.RegularExpressions;

namespace JuninComercial.Desktop.Validation;

/// <summary>
/// Reglas de validación de formato para Country. Separado del code-behind de la
/// ventana para poder testearlo/reusarlo sin depender de WPF.
/// No valida unicidad de código: eso requiere acceso a la base y se resuelve
/// en el repositorio (CountryRepository.ExisteCodigo), fuera de esta clase.
/// </summary>
public static partial class CountryValidator
{
    public const int NombreMinLength = 2;
    public const int NombreMaxLength = 100;

    // ISO 3166-1 alpha-2: exactamente 2 letras mayúsculas (AR, UY, BR, US...).
    [GeneratedRegex("^[A-Z]{2}$")]
    private static partial Regex CodigoIso2Regex();

    public static List<string> ValidateFormat(string? nombre, string? codigo)
    {
        var errors = new List<string>();

        nombre = nombre?.Trim() ?? string.Empty;
        codigo = codigo?.Trim().ToUpperInvariant() ?? string.Empty;

        if (string.IsNullOrEmpty(nombre))
        {
            errors.Add("El nombre es obligatorio.");
        }
        else if (nombre.Length < NombreMinLength || nombre.Length > NombreMaxLength)
        {
            errors.Add($"El nombre debe tener entre {NombreMinLength} y {NombreMaxLength} caracteres.");
        }

        if (string.IsNullOrEmpty(codigo))
        {
            errors.Add("El código es obligatorio.");
        }
        else if (!CodigoIso2Regex().IsMatch(codigo))
        {
            errors.Add("El código debe tener exactamente 2 letras (formato ISO 3166-1 alpha-2, ej: AR, UY, BR).");
        }

        return errors;
    }
}
