using System;
using System.IO;
using System.Net;
using System.Text;
using Azure;

class HttpServer
{
    private string addr;
    private HttpListener listener;
    private string webRootDirectory;

    private bool isRunning;
    public bool IsRunning => isRunning;

    Thread serverThread;

    public HttpServer(string ip = "localhost", string port = "8080")
    {
        this.addr = $"http://{ip}:{port}/";
        this.listener = new HttpListener();
        this.listener.Prefixes.Add(addr);
        this.webRootDirectory = $@"{Utils.CurrentDir}\Pages\";

        this.serverThread = new Thread(new ThreadStart(ThreadLoop));
    }

    public void Start()
    {
        this.isRunning = true;
        this.serverThread.Start();
    }

    public void Stop()
    {
        this.isRunning = false;
        listener.Abort();
    }

    private void ThreadLoop()
    {
        try
        {
            listener.Start();
            Console.WriteLine($"Server started on {addr} .");

            while (this.isRunning)
            {
                // Wait for listener
                HttpListenerContext context = listener.GetContext();

                if (context == null) break;

                // Handle connection
                HttpListenerResponse response = context.Response;

                if (context.Request.Url == null) return;
                string requestedFile = context.Request.Url.AbsolutePath.TrimStart('/');

                if (string.IsNullOrEmpty(requestedFile))
                {
                    requestedFile = "index.html";
                }

                this.HandleRequest(requestedFile, response);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        isRunning = false;
    }

    public bool IsValidFile(string requestedFile)
    {
        // Create the full file path
        string filePath = Path.Combine(webRootDirectory, requestedFile);
        return File.Exists(filePath);
    }

    public bool SendRespondFile(string requestedFile, HttpListenerResponse response)
    {
        // Create the full file path
        string filePath = Path.Combine(webRootDirectory, requestedFile);

        if (IsValidFile(filePath))
        {
            // Set file type
            string fileExtension = Path.GetExtension(filePath).ToLower();

            if (fileExtension == ".html" || fileExtension == ".htm")
            {
                response.ContentType = "text/html";
            }
            else
            {
                response.ContentType = "application/octet-stream";
            }

            // Create the response
            byte[] fileBytes = File.ReadAllBytes(filePath);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentLength64 = fileBytes.Length;
            response.OutputStream.Write(fileBytes, 0, fileBytes.Length);

            return true;
        }
        else
        {
            // If the file doesn't exist, return a 404 error
            string notFoundMessage = "<html><body><h1>404 Not Found</h1><p>The requested file was not found on the server.</p></body></html>";
            byte[] notFoundBytes = Encoding.UTF8.GetBytes(notFoundMessage);

            response.ContentType = "text/html";
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.ContentLength64 = notFoundBytes.Length;
            response.OutputStream.Write(notFoundBytes, 0, notFoundBytes.Length);
        }

        return false;
    }

    public bool SendRespondString(string text, HttpListenerResponse response)
    {
        try
        {
            byte[] notFoundBytes = Encoding.UTF8.GetBytes(text);

            response.StatusCode = (int)HttpStatusCode.Accepted;
            response.ContentLength64 = notFoundBytes.Length;
            response.ContentType = "text/html";
            response.OutputStream.Write(notFoundBytes, 0, notFoundBytes.Length);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public virtual void HandleRequest(string request, HttpListenerResponse response)
    {
        SendRespondFile(request, response);
    }
}
