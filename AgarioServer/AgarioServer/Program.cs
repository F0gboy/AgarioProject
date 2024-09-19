using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class Server
{
    private static Dictionary<Guid, PlayerInfo> playerStates = new Dictionary<Guid, PlayerInfo>();
    static List<TcpClient> allClients = new List<TcpClient>();

    public static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 12000);
        server.Start();
        Console.WriteLine("Server started... listening on port 12000");

        System.Timers.Timer broadcastTimer = new System.Timers.Timer(100); // 100ms interval
        broadcastTimer.Elapsed += (sender, e) => BroadcastPlayerStates();
        broadcastTimer.Start();


        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        Guid clientId = Guid.NewGuid();
        Console.WriteLine($"Client {clientId} connected.");
        allClients.Add(client);
        StreamWriter writer = new StreamWriter(client.GetStream());
        StreamReader reader = new StreamReader(client.GetStream());

        // Request and receive player's name (optional)
        //writer.WriteLine("What is your name?");
        //writer.Flush();
        //string name = reader.ReadLine();  // For now, you can ignore this, or use it later for displaying player names

        try
        {
            while (client.Connected)
            {
                string message = reader.ReadLine(); // Expect JSON with player position and size
                if (message != null)
                {
                    Console.WriteLine($"Received message from {clientId}: {message}");

                    // Deserialize the JSON to PlayerInfo
                    PlayerInfo playerData = JsonConvert.DeserializeObject<PlayerInfo>(message);

                    // Store player info
                    if (playerStates.ContainsKey(clientId))
                    {
                        playerStates[clientId] = playerData;
                    }
                    else
                    {
                        playerStates.Add(clientId, playerData);
                    }

                    // Broadcast updated player states to all clients
                    BroadcastPlayerStates();
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
            playerStates.Remove(clientId);
        }
    }

    private static void BroadcastPlayerStates()
    {
        lock (playerStates)
        {
            string allPlayersJson = JsonConvert.SerializeObject(playerStates.Values.ToList());

            foreach (TcpClient client in allClients)
            {
                if (client.Connected)
                {
                    try
                    {
                        StreamWriter writer = new StreamWriter(client.GetStream());
                        writer.WriteLine(allPlayersJson);
                        writer.Flush();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error broadcasting to client: {ex.Message}");
                    }
                }
            }
        }
    }

    private static void MessageAll(string message)
    {
        // Implementation for broadcasting messages to all clients
    }
}

public class PlayerInfo
{
    public Guid Id { get; set; } // Unique player ID
    public int X { get; set; }
    public int Y { get; set; }
    public int Size { get; set; }
}

