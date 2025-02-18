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

        string query = QueryMaker.Alter_Talbe.With_Name("oldname")
            .Rename_Table("newname")
            .Add_Column("col5", "int")
            .Add_Column("col7", "int")
            .Rename_Column("col7", "col8")
            .Drop_Column("col1").ExecuteNonQuery();

        System.Console.WriteLine(query);
    }
}
