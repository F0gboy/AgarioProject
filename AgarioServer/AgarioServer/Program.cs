using System.Net.Sockets;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Text;
using System.Web;

TcpListener server = new TcpListener(IPAddress.Any, 12000);
server.Start();
Console.WriteLine("Server started... listening on port 12000");
List<TcpClient> allClients = new List<TcpClient>();

HttpClient httpClient = new HttpClient();

while (true)
{
    TcpClient client = server.AcceptTcpClient();
    Thread clientThread = new Thread(() => HandleClient(client));
    clientThread.Start();
}

void MessageAll(string message)
{
    foreach (TcpClient client in allClients)
    {
        StreamWriter writer = new StreamWriter(client.GetStream());
        writer.WriteLine(message);
        writer.Flush();
    }
}

void HandleClient(TcpClient client)
{
    Guid clientId = Guid.NewGuid();
    Console.WriteLine($"Client {clientId} connected.");
    allClients.Add(client);

    StreamWriter writer = new StreamWriter(client.GetStream());

    writer.WriteLine("What is your name?");
    writer.Flush();

    StreamReader reader = new StreamReader(client.GetStream());
    string name = reader.ReadLine();

    try
    {
        while (client.Connected)
        {
            string message = reader.ReadLine();
            if (message != null)
            {
                Console.WriteLine($"Received message from {name}: {message}");
                MessageAll(name + ": " + message);

                // Call the REST API to log the message
                LogMessageToRestService(name, message);

                writer.Flush();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception occurred for client {clientId}: {ex.Message}");
    }
    finally
    {
        Console.WriteLine($"Client disconnected: {clientId}");
        client.Dispose();
    }
}

async Task LogMessageToRestService(string username, string message)
{
    var formData = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("User", username),
        new KeyValuePair<string, string>("Message", message),
        new KeyValuePair<string, string>("Timestamp", DateTime.UtcNow.ToString("o"))
    };

    var content = new FormUrlEncodedContent(formData);

    try
    {
        HttpResponseMessage response = await httpClient.PostAsync("http://localhost:5000/api/logs", content);
        response.EnsureSuccessStatusCode();
        Console.WriteLine("Message logged successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error logging message to REST service: {ex.Message}");
    }
}
