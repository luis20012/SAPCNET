using System.IO;
using System.Text.Json;
using MySqlConnector;

namespace JuninComercial.Desktop.Data;

/// <summary>
/// Equivalente a org.junincomercial.util.DatabaseConnection del proyecto JavaFX original.
/// A diferencia de la versión Java (que hardcodeaba usuario/clave en el código fuente),
/// acá la cadena de conexión se lee de appsettings.json para no versionar credenciales
/// dentro del .cs.
/// </summary>
public static class DatabaseConnectionFactory
{
    private static readonly string ConnectionString = LoadConnectionString();

    public static MySqlConnection Create()
    {
        var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    private static string LoadConnectionString()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "No se encontró appsettings.json junto al ejecutable. " +
                "Verificá que el archivo tenga 'Copiar si es más reciente' configurado.", path);
        }

        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream);

        return doc.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("Default")
            .GetString()
            ?? throw new InvalidOperationException("ConnectionStrings:Default no está configurado en appsettings.json.");
    }
}
