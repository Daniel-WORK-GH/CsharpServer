using System.Data;
using Microsoft.Data.Sqlite;
using SQLitePCL;

public class Database : QueryMaker
{
    private SqliteConnection connection;

    public Database(string database_name, string dir = "Databases\\", bool force_create_dir = true, bool force_create_file = true)
    {
        string filePath = $"{dir}{database_name}.sqlite";
        string connectionString = $"Data Source={filePath}";

        if (!Directory.Exists(dir))
        {
            if (force_create_dir) Directory.CreateDirectory(dir);
            else throw new ArgumentException($"The given directory does not exist, consider setting 'force_create_dir' to True.\n(PATH = {filePath})");
        }

        if (!File.Exists(filePath))
        {
            if (!force_create_file) throw new ArgumentException($"The given database file does not exist, consider setting 'force_create_file' to True.\n(PATH = {filePath})");
        }

        connection = new SqliteConnection(connectionString);
        connection.Open();

        this.SetConnection(connection);
    }

    public void ExecuteNonQuery(string slq_query)
    {
        using (var command = connection.CreateCommand())
        {
            try
            {
                command.CommandText = slq_query;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }

    public override void Dispose()
    {
        connection.Dispose();
    }
}