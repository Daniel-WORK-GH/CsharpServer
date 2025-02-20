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

        System.Console.WriteLine(db.Create_table.With_Name("temp32").Add_Column("c1", "int").ExecuteNonQuery());
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
