using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json; // If you need this for JSON serialization/deserialization

public class Server
{
    private static Dictionary<Guid, PlayerInfo> playerStates = new Dictionary<Guid, PlayerInfo>();
    private static List<TcpClient> allClients = new List<TcpClient>();

    public static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 12000);
        server.Start();
        Console.WriteLine("Server started... listening on port 12000");

        while (true)
        {
            try
            {
                TcpClient client = server.AcceptTcpClient();
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting client: {ex.Message}");
            }
        }
    }

    private static void HandleClient(TcpClient client)
    {
        Guid clientId = Guid.NewGuid();
        lock (allClients)
        {
            allClients.Add(client);
        }

        StreamWriter writer = new StreamWriter(client.GetStream());
        StreamReader reader = new StreamReader(client.GetStream());

        writer.WriteLine("What is your name?");
        writer.Flush();

        string name = reader.ReadLine(); // This line is now redundant and can be removed

        PlayerInfo playerInfo = new PlayerInfo
        {
            Id = clientId,
            X = 100, // starting X
            Y = 100, // starting Y
            Size = 20 // starting size
        };

        playerStates[clientId] = playerInfo;

        try
        {
            while (client.Connected)
            {
                string message = reader.ReadLine();
                if (message != null)
                {
                    // Deserialize the player data sent from the client
                    PlayerInfo updatedPlayer = JsonConvert.DeserializeObject<PlayerInfo>(message);

                    // Update the server's state for this player
                    playerStates[clientId] = updatedPlayer;

                    // Broadcast this player's updated state to all other players
                    BroadcastPlayerStateToAll();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        finally
        {
            lock (allClients)
            {
                allClients.Remove(client);
            }
            playerStates.Remove(clientId);
            client.Dispose();
        }
    }

    private static void BroadcastPlayerStateToAll()
    {
        string allPlayersJson = JsonConvert.SerializeObject(playerStates.Values.ToList());

        lock (allClients)
        {
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
}

public class PlayerInfo
{
    public Guid Id { get; set; } // Use Guid as the player identifier
    public int X { get; set; }
    public int Y { get; set; }
    public int Size { get; set; }
}
