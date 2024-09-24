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

    private static List<DotInfo> dots = new List<DotInfo>(); // Store dots
    private static Random random = new Random();

    private static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 12000);
        server.Start();
        Console.WriteLine("Server started... listening on port 12000");

        // Start a timer to broadcast player states and dots to all clients
        System.Timers.Timer broadcastTimer = new System.Timers.Timer(100); // 100ms interval
        broadcastTimer.Elapsed += (sender, e) => BroadcastPlayerStatesAndDots();
        broadcastTimer.Start();

        // Start another timer to spawn dots periodically
        System.Timers.Timer spawnDotsTimer = new System.Timers.Timer(2000); // Spawn dots every 2 seconds
        spawnDotsTimer.Elapsed += (sender, e) => SpawnDots();
        spawnDotsTimer.Start();

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private static void SpawnDots()
    {
        lock (dots)
        {
            // Spawn a new dot with random location and size
            DotInfo newDot = new DotInfo
            {
                X = random.Next(0, 950), // Assuming 950 is the width limit
                Y = random.Next(0, 900), // Assuming 900 is the height limit
                Size = random.Next(10, 20) // Random size between 10 and 20
            };
            dots.Add(newDot);

            // Log for debugging
            Console.WriteLine($"Dot spawned at ({newDot.X}, {newDot.Y}) with size {newDot.Size}");
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
                    BroadcastPlayerStatesAndDots();
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

    private static void BroadcastPlayerStatesAndDots()
    {
        lock (playerStates)
        {
            // Serialize both player states and dots together
            var broadcastData = new
            {
                Players = playerStates.Values.ToList(),
                Dots = dots
            };

            string broadcastJson = JsonConvert.SerializeObject(broadcastData);

            foreach (TcpClient client in allClients)
            {
                if (client.Connected)
                {
                    try
                    {
                        StreamWriter writer = new StreamWriter(client.GetStream());
                        writer.WriteLine(broadcastJson);
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

public class DotInfo
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Size { get; set; }
}