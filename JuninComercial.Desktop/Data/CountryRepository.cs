using System.Collections.Generic;
using JuninComercial.Desktop.Models;
using MySqlConnector;

namespace JuninComercial.Desktop.Data;

/// <summary>
/// Equivalente a org.junincomercial.dao.CountryDAO del proyecto JavaFX original.
/// Mismas cuatro operaciones (listar, insertar, actualizar, eliminar), mismo SQL crudo.
/// </summary>
public class CountryRepository
{
    public List<Country> ListarTodas()
    {
        var lista = new List<Country>();
        const string sql = "SELECT id_country, name, code FROM country";

        try
        {
            using var conn = DatabaseConnectionFactory.Create();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Country
                {
                    IdCountry = reader.GetInt32("id_country"),
                    Name = reader.GetString("name"),
                    Code = reader.GetString("code"),
                });
            }
        }
        catch (MySqlException e)
        {
            System.Diagnostics.Debug.WriteLine($"Error al listar países: {e.Message}");
        }

        return lista;
    }

    /// <returns>true si se insertó correctamente; false si hubo un error de base de datos.</returns>
    public bool Insertar(Country c)
    {
        const string sql = "INSERT INTO country (name, code) VALUES (@name, @code)";

        try
        {
            using var conn = DatabaseConnectionFactory.Create();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", c.Name);
            cmd.Parameters.AddWithValue("@code", c.Code);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (MySqlException e)
        {
            System.Diagnostics.Debug.WriteLine($"Error al insertar país: {e.Message}");
            return false;
        }
    }

    /// <returns>true si se actualizó correctamente; false si hubo un error de base de datos.</returns>
    public bool Actualizar(Country c)
    {
        const string sql = "UPDATE country SET name = @name, code = @code WHERE id_country = @id";

        try
        {
            using var conn = DatabaseConnectionFactory.Create();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", c.Name);
            cmd.Parameters.AddWithValue("@code", c.Code);
            cmd.Parameters.AddWithValue("@id", c.IdCountry);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (MySqlException e)
        {
            System.Diagnostics.Debug.WriteLine($"Error al actualizar país: {e.Message}");
            return false;
        }
    }

    /// <returns>true si se eliminó correctamente; false si hubo un error de base de datos.</returns>
    public bool Eliminar(int idCountry)
    {
        const string sql = "DELETE FROM country WHERE id_country = @id";

        try
        {
            using var conn = DatabaseConnectionFactory.Create();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idCountry);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (MySqlException e)
        {
            System.Diagnostics.Debug.WriteLine($"Error al eliminar país: {e.Message}");
            return false;
        }
    }

    /// <summary>¿Hay otro país con este código (sin distinguir mayúsculas)?</summary>
    /// <param name="excludeId">Id a excluir de la comparación (el propio registro al editar).</param>
    /// <returns>null si hubo un error de base de datos (no se pudo verificar).</returns>
    public bool? ExisteCodigo(string codigo, int? excludeId)
    {
        const string sql = """
            SELECT COUNT(*) FROM country
            WHERE UPPER(code) = UPPER(@code)
              AND (@excludeId IS NULL OR id_country <> @excludeId)
            """;

        return Contar(sql, "validar código de país",
            ("@code", codigo), ("@excludeId", excludeId)) is long n ? n > 0 : null;
    }

    /// <summary>¿Hay otro país con este nombre (sin distinguir mayúsculas)?</summary>
    /// <param name="excludeId">Id a excluir de la comparación (el propio registro al editar).</param>
    /// <returns>null si hubo un error de base de datos (no se pudo verificar).</returns>
    public bool? ExisteNombre(string nombre, int? excludeId)
    {
        const string sql = """
            SELECT COUNT(*) FROM country
            WHERE UPPER(name) = UPPER(@name)
              AND (@excludeId IS NULL OR id_country <> @excludeId)
            """;

        return Contar(sql, "validar nombre de país",
            ("@name", nombre), ("@excludeId", excludeId)) is long n ? n > 0 : null;
    }

    /// <summary>¿El país sigue existiendo? (por si otro usuario lo borró mientras la ventana estaba abierta)</summary>
    /// <returns>null si hubo un error de base de datos (no se pudo verificar).</returns>
    public bool? Existe(int idCountry)
    {
        const string sql = "SELECT COUNT(*) FROM country WHERE id_country = @id";

        return Contar(sql, "validar existencia de país", ("@id", idCountry)) is long n ? n > 0 : null;
    }

    /// <summary>
    /// ¿Hay provincias que apuntan a este país? Sin este control quedarían
    /// provincias huérfanas al eliminar.
    /// </summary>
    /// <returns>Cantidad de provincias, o null si hubo un error de base de datos.</returns>
    public long? ContarProvincias(int idCountry)
    {
        const string sql = "SELECT COUNT(*) FROM province WHERE id_country = @id";

        return Contar(sql, "contar provincias del país", ("@id", idCountry));
    }

    private static long? Contar(string sql, string operacion, params (string Name, object? Value)[] parameters)
    {
        try
        {
            using var conn = DatabaseConnectionFactory.Create();
            using var cmd = new MySqlCommand(sql, conn);
            foreach (var (name, value) in parameters)
            {
                cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
            }

            return Convert.ToInt64(cmd.ExecuteScalar());
        }
        catch (MySqlException e)
        {
            System.Diagnostics.Debug.WriteLine($"Error al {operacion}: {e.Message}");
            return null;
        }
    }
}
