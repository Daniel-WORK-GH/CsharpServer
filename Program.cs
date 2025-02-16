using System.Net;
using System.Text;

class Program
{
    public static void Main()
    {
        HttpServer server = new MyServer();

        server.Start();
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
