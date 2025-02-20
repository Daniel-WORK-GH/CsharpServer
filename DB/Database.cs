using System.Data;
using Microsoft.Data.Sqlite;
using SQLitePCL;

public class Database : QueryMaker
{
    protected readonly SqliteConnection connection;

    protected readonly string database_name;

    protected readonly string dir;

    protected readonly string database_path;

    public Database(string database_name, string dir = "Databases\\", bool force_create_dir = true, bool force_create_file = true)
    {
        this.database_name = database_name;
        this.dir = dir;

        string filePath = $"{dir}{database_name}.sqlite";
        string connectionString = $"Data Source={filePath}";

        this.database_path = filePath;

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

    public void ExecuteReader(string sql_query)
    {
        using (var command = new SqliteCommand(sql_query, connection))
        {
            using (var reader = command.ExecuteReader())
            {
                int columnCount = reader.FieldCount;
                for (int i = 0; i < columnCount; i++)
                {
                    string columnName = reader.GetName(i);
                    Type columnType = reader.GetFieldType(i);
                    Console.Write($"{columnName} ({columnType.Name})\t");
                }
                Console.WriteLine("\n" + new string('-', 40));

                while (reader.Read())
                {
                    for (int i = 0; i < columnCount; i++)
                    {
                        Console.Write(reader[i] + "\t");
                    }
                    Console.WriteLine();
                }
            }
        }

    }

    public override void Dispose()
    {
        connection.Dispose();
    }
}