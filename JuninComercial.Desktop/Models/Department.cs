namespace JuninComercial.Desktop.Models;

/// <summary>
/// Equivalente a org.junincomercial.model.Department del proyecto JavaFX original.
/// Tipos ajustados a la tabla real: zipcode e id_province son INT NULL en MySQL.
/// </summary>
public class Department
{
    public int IdDepartment { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Zipcode { get; set; }
    public int? IdProvince { get; set; }
    public int? IdUserCreate { get; set; }
    public int? IdUserUpdate { get; set; }
    public DateTime? DateCreate { get; set; }
    public DateTime? DateUpdate { get; set; }

    /// <summary>Nombre de la provincia (LEFT JOIN al listar). No es una columna de department.</summary>
    public string? ProvinceName { get; set; }

    /// <summary>Lo que muestra la tabla: el nombre, o el ID si la provincia no existe en la tabla province.</summary>
    public string ProvinciaDisplay => ProvinceName ?? IdProvince?.ToString() ?? string.Empty;
}
