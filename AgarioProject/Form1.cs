using Microsoft.VisualBasic.Devices;
using System.Net.Sockets;
using Newtonsoft.Json;
using Timer = System.Windows.Forms.Timer;
using System.Diagnostics;

namespace AgarioProject
{
    public partial class Form1 : Form
    {
        private Dictionary<Guid, PictureBox> otherPlayers = new Dictionary<Guid, PictureBox>();
        public Random random = new Random();
        public List<PictureBox> dots = new List<PictureBox>();
        public int x;
        public int y;
        public int spawnTimer = 10;
        private Guid myPlayerId;
        Color[] colors = { Color.AliceBlue, Color.Beige, Color.BlueViolet, Color.BurlyWood, Color.Crimson, Color.Cyan, Color.DarkGoldenrod, Color.DarkOrange, Color.DarkSeaGreen, Color.DeepPink, Color.HotPink, Color.LimeGreen };

        public class PlayerInfo
        {
            public Guid Id { get; set; }  // Ensure this is a unique ID
            public int X { get; set; }
            public int Y { get; set; }
            public int Size { get; set; }

            // Constructor to initialize the player info with a unique Id
            public PlayerInfo()
            {
                Id = Guid.NewGuid();  // Assign a new GUID when a player is created
            }
        }

        public TcpClient client;
        private StreamWriter writer;
        private StreamReader reader;
        private Thread receiveThread;

        public Form1()
        {
            InitializeComponent();

            try
            {
                // Initialize and connect to the server
                client = new TcpClient();
                client.Connect("localhost", 12000); // Ensure the server is running at this IP and port

                // Initialize the writer to send messages to the server
                writer = new StreamWriter(client.GetStream());
                reader = new StreamReader(client.GetStream());

                InitializePlayer(); // Ensure this is called

                Debug.WriteLine($"My player ID: {myPlayerId}"); // Print the player ID to verify

                // Start the thread to receive messages
                receiveThread = new Thread(() => ReceiveMessages(client));
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}");
                Debug.WriteLine($"Error connecting to server: {ex.Message}");
            }
        }

        private void InitializePlayer()
        {
            if (myPlayerId == Guid.Empty)
            {
                myPlayerId = Guid.NewGuid(); // Generate a new unique ID
                Console.WriteLine($"Generated client ID: {myPlayerId}"); // Debugging print
            }
        }

        private void StartSendingPlayerUpdates()
        {
            Timer sendTimer = new Timer();
            sendTimer.Interval = 100; // 100ms update interval
            sendTimer.Tick += (sender, e) =>
            {
                SendPlayerState(); // Send current player state to server
            };
            sendTimer.Start();
        }

        private void SendPlayerState()
        {
            if (client.Connected)
            {
                PlayerInfo myState = new PlayerInfo
                {
                    Id = myPlayerId, // Reuse the previously generated unique player ID
                    X = pictureBox1.Location.X,  // Player's current X position
                    Y = pictureBox1.Location.Y,  // Player's current Y position
                    Size = pictureBox1.Width     // Player size
                };

                string myStateJson = JsonConvert.SerializeObject(myState);

                try
                {
                    writer.WriteLine(myStateJson); // Send serialized player state to the server
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error sending player state: {ex.Message}");
                    Debug.WriteLine($"Error sending player state: {ex.Message}");
                }
            }
        }

        private void ReceiveMessages(TcpClient client)
        {
            StreamReader reader = new StreamReader(client.GetStream());

            try
            {
                while (client.Connected)
                {
                    string message = reader.ReadLine();
                    if (message != null)
                    {
                        // Deserialize the JSON into a list of PlayerInfo
                        List<PlayerInfo> allPlayers = JsonConvert.DeserializeObject<List<PlayerInfo>>(message);

                        // Update the client’s rendering based on allPlayers
                        UpdateGameWithPlayerData(allPlayers);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error receiving message: {ex.Message}");
                Debug.WriteLine($"Error receiving message: {ex.Message}");
            }
        }

        private void UpdateGameWithPlayerData(List<PlayerInfo> allPlayers)
        {
            // Clear all previous player representations (if necessary)

            foreach (PlayerInfo player in allPlayers)
            {
                if (player.Id != myPlayerId) // Skip the current player to avoid overwriting local movement
                {
                    // Update or create a PictureBox representing each player
                    PictureBox otherPlayer = GetOrCreatePlayerPictureBox(player.Id);

                    otherPlayer.Location = new Point(player.X, player.Y);
                    otherPlayer.Width = player.Size;
                    otherPlayer.Height = player.Size;
                }
            }
        }

        private PictureBox GetOrCreatePlayerPictureBox(Guid playerId)
        {
            // Check if the player already exists, otherwise create a new one
            PictureBox playerBox = otherPlayers.ContainsKey(playerId) ? otherPlayers[playerId] : null;

            if (playerBox == null)
            {
                playerBox = new PictureBox
                {
                    BackColor = Color.Red // Assign a color for the other player
                };
                otherPlayers[playerId] = playerBox;
                this.Controls.Add(playerBox);
            }

            return playerBox;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialization code here
        }

        private void Expand(PictureBox obj, int amount)
        {
            obj.Width += amount;
            obj.Height += amount;
        }

        private void MoveToMouse(int x, int y, PictureBox obj, int speed)
        {
            Point position = new Point(x, y);
            Point newPosition = obj.Location;
            int pX = position.X - (obj.Width / 2);
            int pY = position.Y - (obj.Height / 2);
            position = new Point(pX, pY);

            if (obj.Location.X > position.X && obj.Location.X - position.X > 6)
            {
                newPosition = new Point(obj.Location.X - speed, obj.Location.Y);
            }
            else if (position.X - obj.Location.X > 6)
            {
                newPosition = new Point(obj.Location.X + speed, obj.Location.Y);
            }

            if (obj.Location.Y > position.Y && obj.Location.Y - position.Y > 6)
            {
                newPosition = new Point(newPosition.X, obj.Location.Y - speed);
            }
            else if (position.Y - obj.Location.Y > 6)
            {
                newPosition = new Point(newPosition.X, obj.Location.Y + speed);
            }

            if (newPosition.X > 950)
            {
                newPosition.X = 950;
            }

            if (newPosition.Y > 900)
            {
                newPosition.Y = 900;
            }

            if (newPosition.X < 0)
            {
                newPosition.X = 0;
            }

            if (newPosition.Y < 0)
            {
                newPosition.Y = 0;
            }

            obj.Location = newPosition;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            // Handle mouse movement if necessary
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Point mouse = PointToClient(Cursor.Position);
            MoveToMouse(mouse.X, mouse.Y, pictureBox1, 3);

            PlayerInfo playerData = new PlayerInfo
            {
                Id = myPlayerId,    // Use the same unique ID for broadcasting
                X = pictureBox1.Location.X,
                Y = pictureBox1.Location.Y,
                Size = pictureBox1.Width // or Height, since they're equal
            };

            // Serialize the player data and send it to the server
            string jsonData = JsonConvert.SerializeObject(playerData);
            writer.WriteLine(jsonData);
            writer.Flush();

            // Rest of your game logic (like spawning dots, detecting collisions, etc.)
        }

        private void BroadcastInfo()
        {
            PlayerInfo playerData = new PlayerInfo
            {
                Id = myPlayerId,    // Use the same unique ID for broadcasting
                X = pictureBox1.Location.X,
                Y = pictureBox1.Location.Y,
                Size = pictureBox1.Width // or Height, since they're equal
            };

            string jsonData = JsonConvert.SerializeObject(playerData);
            writer.WriteLine(jsonData);
            writer.Flush();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString(
                    pictureBox1.Width.ToString(),
                    myFont,
                    Brushes.Black,
                    new Point(
                        pictureBox1.Width / 2 - (TextRenderer.MeasureText(pictureBox1.Width.ToString(), myFont).Width / 2) + 3,
                        pictureBox1.Height / 2 - (TextRenderer.MeasureText(pictureBox1.Width.ToString(), myFont).Height / 2)));
            }
        }

        private void SpawnDots()
        {
            PictureBox new_dot = new PictureBox
            {
                Height = 10,
                Width = 10,
                BackColor = colors[random.Next(0, colors.Length)]
            };

            x = random.Next(0, this.ClientSize.Width - new_dot.Width);
            y = random.Next(0, this.ClientSize.Height - new_dot.Height);

            new_dot.Location = new Point(x, y);

            dots.Add(new_dot);
            this.Controls.Add(new_dot);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Handle PictureBox click if necessary
        }
    }
}
