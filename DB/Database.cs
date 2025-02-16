using System;
using System.Globalization;
using System.Security.AccessControl;
using Microsoft.Data.Sqlite;

public sealed class Database : IDisposable
{
    private SqliteConnection connection;

    public Database(string file_name)
    {
        // Database file path
        string file_path = Utils.CurrentDir + $"\\Databases\\{file_name}.sqlite";

        // Connection string to SQLite (use the file path to your .db file)
        string connectionString = $"Data Source={file_path}";

        connection = new SqliteConnection(connectionString);
        connection.Open();
    }

    public NonQueryResult ExecuteNonQuery(string query)
    {
        try
        {
            using (var command = new SqliteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
            return new(true, query, "");
        }
        catch (Exception ex)
        {
            return new(false, query, ex.Message);
        }
    }

    public void Dispose()
    {
        connection?.Dispose();
    }
}
