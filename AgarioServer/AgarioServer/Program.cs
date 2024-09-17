using System.Net.Sockets;
using System.Net;
using AgarioServer;

TcpListener server = new TcpListener(IPAddress.Any, 12000);
server.Start();
Console.WriteLine("Server started... listening on port 12000");
List<TcpClient> allClients = new List<TcpClient>();

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
    // Send client name to the client

    StreamWriter writer = new StreamWriter(client.GetStream());

    writer.WriteLine("What is your name?");
    writer.Flush();

    // Receive and send messages
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
                MessageAll(name + ":" + " " + message);
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
