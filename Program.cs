using System.Dynamic;
using System.Net;
using System.Text;

class Program
{
    public static void Main()
    {
        HttpServer server = new HttpServer(use_threading: false);
        server.Start();

        string input;
        do
        {
            input = Console.ReadLine()!.ToLower();
        } while (input != "q");

        server.Stop();
    }
}
