namespace JuninComercial.Desktop.Models;

/// <summary>
/// Equivalente a org.junincomercial.model.Country del proyecto JavaFX original.
/// </summary>
public class Country
{
    public int IdCountry { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int? IdUserUpdate { get; set; }
    public int? IdUserCreate { get; set; }
    public DateTime? DateUpdate { get; set; }
    public DateTime? DateCreate { get; set; }
}
