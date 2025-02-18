using System.Net;
using System.Text;

class Program
{
    public static void Main()
    {
        using Database db = new Database(
            database_name: "Database",
            dir: "Databases\\",
            force_create_dir: true,
            force_create_file: true
        );

    }
}
