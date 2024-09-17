using System.Net.Sockets;
using System.Net;
using AgarioServer;

TcpListener server = new TcpListener(IPAddress.Any, 12000);
server.Start();
Console.WriteLine("Server started... listening on port 12000");
List<TcpClient> allClients = new List<TcpClient>();
List<Player> allPlayers = new List<Player>();
List<Food> allFood = new List<Food>();


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
    string name = ""; // Placeholder for player name
    Player player = new Player(clientId, name);

    StreamWriter writer = new StreamWriter(client.GetStream());
    StreamReader reader = new StreamReader(client.GetStream());

    writer.WriteLine("What is your name?");
    writer.Flush();
    player.Name = reader.ReadLine();

    allClients.Add(client);
    allPlayers.Add(player); // Add player to the game

    while (client.Connected)
    {
        string message = reader.ReadLine();
        if (message != null)
        {
            // Parse message for movement data
            var parts = message.Split(':');
            if (parts[0] == "MOVE") // Example: MOVE:up
            {
                string direction = parts[1].Trim();
                //UpdatePlayerPosition(player, direction);
            }

            // Broadcast new player positions to all clients
            BroadcastGameState();
        }
    }
}

void BroadcastGameState()
{
    foreach (TcpClient client in allClients)
    {
        StreamWriter writer = new StreamWriter(client.GetStream());
        foreach (Player player in allPlayers)
        {

        }
        foreach (Food food in allFood)
        {

        }
        writer.Flush();
    }
}


