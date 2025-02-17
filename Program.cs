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
            .Add_Column("col4", "int")
            .Add_Column("col5", "int")
            .Drop_Column("col2")
            .Rename_Table("newtablename")
            .Rename_Column("col1", "col6")
            .ExecuteNonQuery();

        System.Console.WriteLine(query);
    }

    class MyServer : HttpServer
    {
        public override void HandleRequest(string request, HttpListenerResponse response)
        {
            if (request == "send_hi")
            {
                this.SendRespondString("daniel<br>dan<br>dani<br>dana<br>danielle", response);
            }
        }
    }
}
