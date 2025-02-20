using Microsoft.Data.Sqlite;

public class ServerDatabase : Database
{
    private readonly (string username, string password) auth;

    public ServerDatabase(string database_name, (string username, string password) auth,
        string dir = "Databases\\", bool force_create_dir = true, bool force_create_file = true) : 
        base(database_name, dir, force_create_dir, force_create_file)
    {
        this.auth = auth;
    }

    private string CreateRow(params string[] rowdata)
    {
        string row = "";

        return row;
    }

    private string CloneDatabase() 
    {
        Guid randomname = Guid.NewGuid();
        string filePath = $"{dir}{randomname}.sqlite";

        var command = connection.CreateCommand();
        
        command.CommandText = $"VACUUM INTO '{filePath}';";
        command.ExecuteNonQuery();

        command.CommandText = $"ATTACH DATABASE '{filePath}' AS tempdb;";
        command.ExecuteNonQuery();

        return filePath;
    }

    public string GetDatabasePage(string username, string password, string request)
    {
        if(username != auth.username || password != auth.password) return "<h1>ACCESS DENIED 403.</h1>";
        if(!File.Exists(database_path)) return "<h1>DATABASE NOT FOUND 404.</h1>";

        string clonedatabasename = "";//CloneDatabase();

        List<string> tableNames = new List<string>();
        var command = connection.CreateCommand();
        //command.CommandText = "SELECT name FROM tempdb.sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                tableNames.Add(reader.GetString(0));
            }
        }

        System.Console.WriteLine(request);

        string tables = string.Join("\n", tableNames.Select(x => @$"<a href=""{request}\\{x}"">{x}</a>")); // <a href="#">Link 1</a>

        string filedata = File.ReadAllText("DB\\index.html");
        filedata = filedata.Replace("{tables}", tables);

        return filedata;
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}