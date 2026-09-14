using System.Collections.Generic;
using JuninComercial.Desktop.Models;
using MySqlConnector;

namespace JuninComercial.Desktop.Data;

/// <summary>
/// Equivalente a org.junincomercial.service.DepartmentService del proyecto JavaFX original.
/// Mismas cuatro operaciones (listar, guardar, actualizar, eliminar), con SQL parametrizado.
/// </summary>
public class DepartmentRepository
{
    public List<Department> ListarTodos()
    {
        var lista = new List<Department>();
        const string sql = """
            SELECT d.id_department, d.name, d.zipcode, d.id_province, p.name AS province_name
            FROM department d
            LEFT JOIN province p ON p.id_province = d.id_province
            ORDER BY d.name
            """;

        try
        {
            using var conn = DatabaseConnectionFactory.Create();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            var ordZip = reader.GetOrdinal("zipcode");
            var ordProv = reader.GetOrdinal("id_province");
            var ordProvName = reader.GetOrdinal("province_name");

            while (reader.Read())
            {
                lista.Add(new Department
                {
                    IdDepartment = reader.GetInt32("id_department"),
                    Name = reader.GetString("name"),
                    Zipcode = reader.IsDBNull(ordZip) ? null : reader.GetInt32(ordZip),
                    IdProvince = reader.IsDBNull(ordProv) ? null : reader.GetInt32(ordProv),
                    ProvinceName = reader.IsDBNull(ordProvName) ? null : reader.GetString(ordProvName),
                });
            }
        }
        catch (MySqlException e)
        {
            System.Diagnostics.Debug.WriteLine($"Error al listar departamentos: {e.Message}");
        }

        return lista;
    }

    /// <returns>true si se insertó correctamente; false si hubo un error de base de datos.</returns>
    public bool Insertar(Department d)
    {
        const string sql = """
            INSERT INTO department (name, zipcode, id_province, date_create)
            VALUES (@name, @zipcode, @idProvince, NOW())
            """;

        try
        {
            using var conn = DatabaseConnectionFactory.Create();
            using var cmd = new MySqlCommand(sql, conn);
            AddCommonParameters(cmd, d);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (MySqlException e)
        {
            System.Diagnostics.Debug.WriteLine($"Error al insertar departamento: {e.Message}");
            return false;
        }
    }

    /// <returns>true si se actualizó correctamente; false si hubo un error de base de datos.</returns>
    public bool Actualizar(Department d)
    {
        const string sql = """
            UPDATE department
            SET name = @name, zipcode = @zipcode, id_province = @idProvince, date_update = NOW()
            WHERE id_department = @id
            """;

        try
        {
            using var conn = DatabaseConnectionFactory.Create();
            using var cmd = new MySqlCommand(sql, conn);
            AddCommonParameters(cmd, d);
            cmd.Parameters.AddWithValue("@id", d.IdDepartment);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (MySqlException e)
        {
            System.Diagnostics.Debug.WriteLine($"Error al actualizar departamento: {e.Message}");
            return false;
        }
    }

    /// <returns>true si se eliminó correctamente; false si hubo un error de base de datos.</returns>
    public bool Eliminar(int idDepartment)
    {
        const string sql = "DELETE FROM department WHERE id_department = @id";

        try
        {
            using var conn = DatabaseConnectionFactory.Create();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idDepartment);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (MySqlException e)
        {
            System.Diagnostics.Debug.WriteLine($"Error al eliminar departamento: {e.Message}");
            return false;
        }
    }

    /// <summary>¿Hay otro departamento con este código postal?</summary>
    /// <param name="excludeId">Id a excluir de la comparación (el propio registro al modificar).</param>
    /// <returns>null si hubo un error de base de datos (no se pudo verificar).</returns>
    public bool? ExisteCodigoPostal(int zipcode, int? excludeId)
    {
        const string sql = """
            SELECT COUNT(*) FROM department
            WHERE zipcode = @zipcode
              AND (@excludeId IS NULL OR id_department <> @excludeId)
            """;

        return Contar(sql, "validar código postal",
            ("@zipcode", zipcode), ("@excludeId", excludeId)) is long n ? n > 0 : null;
    }

    /// <summary>¿Hay otro departamento con el mismo nombre (sin distinguir mayúsculas) en la misma provincia?</summary>
    /// <returns>null si hubo un error de base de datos (no se pudo verificar).</returns>
    public bool? ExisteNombreEnProvincia(string nombre, int? idProvince, int? excludeId)
    {
        const string sql = """
            SELECT COUNT(*) FROM department
            WHERE UPPER(name) = UPPER(@name)
              AND id_province <=> @idProvince
              AND (@excludeId IS NULL OR id_department <> @excludeId)
            """;

        return Contar(sql, "validar nombre de departamento",
            ("@name", nombre), ("@idProvince", idProvince), ("@excludeId", excludeId)) is long n ? n > 0 : null;
    }

    /// <returns>null si hubo un error de base de datos (no se pudo verificar).</returns>
    public bool? ExisteProvincia(int idProvince)
    {
        const string sql = "SELECT COUNT(*) FROM province WHERE id_province = @id";

        return Contar(sql, "validar provincia", ("@id", idProvince)) is long n ? n > 0 : null;
    }

    /// <summary>
    /// ¿Hay distritos que apuntan a este departamento? La tabla district no tiene FK,
    /// así que sin este control quedarían distritos huérfanos al eliminar.
    /// </summary>
    /// <returns>Cantidad de distritos, o null si hubo un error de base de datos.</returns>
    public long? ContarDistritos(int idDepartment)
    {
        const string sql = "SELECT COUNT(*) FROM district WHERE id_department = @id";

        return Contar(sql, "contar distritos del departamento", ("@id", idDepartment));
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

    private static void AddCommonParameters(MySqlCommand cmd, Department d)
    {
        cmd.Parameters.AddWithValue("@name", d.Name);
        cmd.Parameters.AddWithValue("@zipcode", (object?)d.Zipcode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@idProvince", (object?)d.IdProvince ?? DBNull.Value);
    }
}
