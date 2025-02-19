public class ServerDatabase : Database
{
    private readonly (string username, string password) auth;

    public ServerDatabase(string database_name, (string username, string password) auth,
        string dir = "Databases\\", bool force_create_dir = true, bool force_create_file = true) : 
        base(database_name, dir, force_create_dir, force_create_file)
    {
        this.auth = auth;
    }

    private string GetDatabasePage(string username, string password)
    {
        if(username != auth.username || password != auth.password) return "<h1>ACCESS DENIED 403.</h1>";

        string rows = " ";

        string page = @"";

        return "";
    }
}