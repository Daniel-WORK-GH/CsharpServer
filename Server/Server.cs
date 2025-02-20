using System;
using System.IO;
using System.Net;
using System.Text;
using Azure;
using Azure.Core.Pipeline;

class HttpServer
{
    private string addr;
    private HttpListener listener;
    private string webRootDirectory;

    private bool isRunning;
    public bool IsRunning => isRunning;

    private ServerDatabase database;

    private bool use_threading;

    Thread serverThread;

    public HttpServer(string ip = "localhost", string port = "8080", bool use_threading = true)
    {
        this.addr = $"http://{ip}:{port}/";
        this.listener = new HttpListener();
        this.listener.Prefixes.Add(addr);
        this.webRootDirectory = $@"{Utils.CurrentDir}\Pages\";

        this.serverThread = new Thread(new ThreadStart(ThreadLoop));

        this.use_threading = use_threading;

        this.database = new ServerDatabase(
            database_name: "Database",
            dir: "Databases\\",
            force_create_dir: true,
            force_create_file: true,
            auth: ("user", "1234")
        );
    }

    public void Start()
    {
        this.isRunning = true;
        
        if(this.use_threading)
        {
            this.serverThread.Start();
        }
        else 
        {
            ThreadLoop();
        }
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

                System.Console.WriteLine($"LOG: User {context.Request.RemoteEndPoint} requested {context.Request.Url}.");

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

    public bool SendRespondDatabase(string requestedFile, HttpListenerResponse response)
    {
        try
        {
            string html = database.GetDatabasePage("user", "1234", requestedFile);
            byte[] bytes = Encoding.UTF8.GetBytes(html);

            response.StatusCode = (int)HttpStatusCode.Accepted;
            response.ContentLength64 = html.Length;
            response.ContentType = "text/html";
            response.OutputStream.Write(bytes, 0, bytes.Length);
            return true;
        }
        catch 
        {
            return false;
        }
    }

    public virtual void HandleRequest(string request, HttpListenerResponse response)
    {   
        if(request.Split('/')[0].ToLower() == "database")
        {
            SendRespondDatabase(request, response);
        }
        else
        {
            SendRespondFile(request, response);
        }
    }
}
