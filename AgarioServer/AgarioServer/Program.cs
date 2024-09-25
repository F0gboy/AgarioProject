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
            // Check if the maximum number of dots has been reached
            if (dots.Count < 20) // Limit to 20 dots, for example
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
            else
            {
                // Optionally, update existing dots or adjust their sizes
                foreach (var dot in dots)
                {
                    // For example, you can randomly increase the size of existing dots
                    dot.Size = random.Next(10, 20);
                }
            }
        }
    }

    private static void HandleClient(TcpClient client)
    {
        Guid clientId = Guid.NewGuid();
        Console.WriteLine($"Client {clientId} connected.");
        allClients.Add(client);
        StreamWriter writer = new StreamWriter(client.GetStream());
        StreamReader reader = new StreamReader(client.GetStream());

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

                    // Handle dot collisions
                    HandleDotCollisions(playerData);

                    // Handle player collisions
                    HandlePlayerCollisions(playerData);

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


    private static void HandlePlayerCollisions(PlayerInfo currentPlayer)
    {
        List<Guid> eatenPlayers = new List<Guid>();

        foreach (var otherPlayer in playerStates.Values)
        {
            if (otherPlayer.Id == currentPlayer.Id) continue; // Skip if it's the same player

            // Calculate distance between players
            int distanceX = Math.Abs((currentPlayer.X + (currentPlayer.Size / 2)) - (otherPlayer.X + (otherPlayer.Size / 2)));
            int distanceY = Math.Abs((currentPlayer.Y + (currentPlayer.Size / 2)) - (otherPlayer.Y + (otherPlayer.Size / 2)));

            // Using Pythagorean theorem to calculate distance
            double distance = Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2)) - (currentPlayer.Size / 2);

            // If player collides and is bigger than the other player, "eat" them
            if (distance < -5 && currentPlayer.Size > otherPlayer.Size)
            {
                // Log the player getting eaten
                Console.WriteLine($"Player {currentPlayer.Id} ate player {otherPlayer.Id}");

                // Add the eaten player to the list
                eatenPlayers.Add(otherPlayer.Id);

                // Increase the current player's size based on the size of the eaten player
                currentPlayer.Size += otherPlayer.Size;
            }
        }

        // Remove eaten players
        foreach (var playerId in eatenPlayers)
        {
            DisconnectPlayer(playerId);  // Disconnect the eaten player
        }
    }


    private static Guid GetClientId(TcpClient client)
    {
        return playerStates.FirstOrDefault(x => allClients.Contains(client)).Key;
    }

    private static void DisconnectPlayer(Guid playerId)
    {
        if (playerStates.ContainsKey(playerId))
        {
            // Remove player from the playerStates dictionary
            playerStates.Remove(playerId);
        }

        // Find the corresponding TcpClient in allClients and close it
        var client = allClients.FirstOrDefault(c =>
        {
            var stream = c.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);
            var playerInfo = playerStates.Values.FirstOrDefault(p => p.Id == playerId);
            return playerInfo != null && playerId == playerInfo.Id;
        });

        if (client != null)
        {
            allClients.Remove(client);  // Remove the TcpClient from the list
            client.Close();             // Close the TcpClient connection
            client.Dispose();           // Dispose of it to free resources
        }

        // Log player disconnection for debugging purposes
        Console.WriteLine($"Player {playerId} has been disconnected after being eaten.");

        // Optionally, broadcast the update to all other players
        BroadcastPlayerStatesAndDots();
    }




    private static void HandleDotCollisions(PlayerInfo player)
    {
        lock (dots)
        {
            var eatenDots = new List<DotInfo>();

            foreach (var dot in dots)
            {
                // Calculate the distance between the player and the dot
                int x3 = Math.Abs((player.X + (player.Size / 2)) - dot.X);
                int y3 = Math.Abs((player.Y + (player.Size / 2)) - dot.Y);

                // Using Pythagorean theorem to calculate the distance
                double distance = Math.Sqrt(Math.Pow(x3, 2) + Math.Pow(y3, 2)) - (player.Size / 2);

                // If the player is close enough to "eat" the dot (within the radius)
                if (distance < -5)
                {
                    // Add dot to eaten list and increase player size
                    eatenDots.Add(dot);
                    player.Size += 1;
                       
                    // Log for debugging
                    Console.WriteLine($"Player {player.Id} ate dot at ({dot.X}, {dot.Y})");
                }
            }

            // Remove all eaten dots from the game
            foreach (var dot in eatenDots)
            {
                dots.Remove(dot);
            }
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
                Dots = dots.Select(dot => new { dot.X, dot.Y, dot.Size }).ToList() // Prepare for serialization
            };

            string broadcastJson = JsonConvert.SerializeObject(broadcastData);

            foreach (TcpClient client in allClients)
            {
                if (client.Connected)
                {
                    try
                    {
                        StreamWriter writer = new StreamWriter(client.GetStream());
                        writer.WriteLine(broadcastJson);  // Sending all players' states including their size
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