using System.Dynamic;
using System.Net;
using System.Text;

class Program
{
    public static void Main()
    {
        HttpServer server = new HttpServer(use_threading: false);
        server.Start();

        Database db = new Database();

        db.Insert<int>().To_Table().Values(new int[]{})

        string input;
        do
        {
            input = Console.ReadLine()!.ToLower();
        } while (input != "q");

        server.Stop();
    }
}
