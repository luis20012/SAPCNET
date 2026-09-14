using System.Collections.Generic;
using JuninComercial.Desktop.Models;
using MySqlConnector;

namespace JuninComercial.Desktop.Data;

/// <summary>
/// Solo lectura de provincias, para llenar el desplegable de Departamentos.
/// </summary>
public class ProvinceRepository
{
    public List<Province> ListarTodas()
    {
        var lista = new List<Province>();
        const string sql = "SELECT id_province, name FROM province ORDER BY name";

        try
        {
            using var conn = DatabaseConnectionFactory.Create();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Province
                {
                    IdProvince = reader.GetInt32("id_province"),
                    Name = reader.GetString("name"),
                });
            }
        }
        catch (MySqlException e)
        {
            System.Diagnostics.Debug.WriteLine($"Error al listar provincias: {e.Message}");
        }

        return lista;
    }
}
