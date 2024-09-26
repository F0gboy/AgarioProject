using Microsoft.VisualBasic.Devices;
using System.Net.Sockets;
using Newtonsoft.Json;
using Timer = System.Windows.Forms.Timer;

namespace AgarioProject
{
    public partial class Form1 : Form
    {
        private Dictionary<Guid, PictureBox> otherPlayers = new Dictionary<Guid, PictureBox>();


        public Random random = new Random();
        public List<PictureBox> dots = new List<PictureBox>();
        public List<PictureBox> enemys = new List<PictureBox>();

        public int x;
        public int y;
        public int spawnTimer = 10;
        private Guid myPlayerId;


        Color[] colors = { Color.AliceBlue, Color.Beige, Color.BlueViolet, Color.BurlyWood, Color.Crimson, Color.Cyan, Color.DarkGoldenrod, Color.DarkOrange, Color.DarkSeaGreen, Color.DeepPink, Color.HotPink, Color.LimeGreen };

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
        public class EnemyInfo
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Size { get; set; }
            public Color Color { get; set; }
        }

        public TcpClient client;
        private StreamWriter writer;
        private StreamReader reader;
        private Thread receiveThread;

        public Form1()
        {
            InitializeComponent();

            // Initialize player ID
            InitializePlayer();

            // Initialize and connect to the server
            client = new TcpClient();
            client.Connect("localhost", 12000);

            // Initialize the writer to send messages to the server
            writer = new StreamWriter(client.GetStream());
            reader = new StreamReader(client.GetStream());

            // Start a new thread to receive messages from the server
            receiveThread = new Thread(ReceivePlayerStates);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }


        private void InitializePlayer()
        {
            // Generate a unique ID for this player
            myPlayerId = Guid.NewGuid();

            RandomColor(pictureBox1);

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
                    Id = myPlayerId, // Send unique player ID
                    X = pictureBox1.Location.X,  // Player's current X position
                    Y = pictureBox1.Location.Y,  // Player's current Y position
                    Size = pictureBox1.Width     // Player size
                };

                string myStateJson = JsonConvert.SerializeObject(myState);

                try
                {
                    writer.WriteLine(myStateJson); // Send serialized player state to server
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending player state: {ex.Message}");
                }
            }
        }



        private void ReceivePlayerStates()
        {
            Task.Run(() =>
            {
                while (client.Connected)
                {
                    try
                    {
                        string message = reader.ReadLine();
                        if (!string.IsNullOrEmpty(message))
                        {
                            // Deserialize the combined data from the server
                            var receivedData = JsonConvert.DeserializeObject<ServerData>(message);

                            // Update player states and dots in the UI
                            UpdatePlayerStates(receivedData.Players);
                            UpdateDots(receivedData.Dots); // New method to handle dots
                            UpdateEnemy(receivedData.Enemy);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error receiving player states: {ex.Message}");
                        break;
                    }
                }
            });
        }

        // Class for deserializing the received data
        public class ServerData
        {
            public List<PlayerInfo> Players { get; set; }
            public List<DotInfo> Dots { get; set; }
            public List<EnemyInfo> Enemy { get; set; }

        }

        private void UpdateEnemy(List<EnemyInfo> serverEnemy)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    UpdateEnemy(serverEnemy);
                });
                return;
            }

            // Keep track of which dots from the server are new
            var enemyIds = new HashSet<string>();

            // Update existing dots or add new ones if they don't exist
            foreach (var enemyInfo in serverEnemy)
            {
                // Create a unique identifier for the dot based on its position
                string enemyId = $"{enemyInfo.X}_{enemyInfo.Y}";
                enemyIds.Add(enemyId);

                // Find if a dot with the same position and size already exists
                var existingDot = dots.FirstOrDefault(d =>
                    d.Location.X == enemyInfo.X &&
                    d.Location.Y == enemyInfo.Y &&
                    d.Width == enemyInfo.Size &&
                    d.Height == enemyInfo.Size);

                if (existingDot != null)
                {
                    // Dot already exists, just update its properties if needed (color, etc.)
                    //existingDot.BackColor = colors[random.Next(0, colors.Length)];
                }
                else
                {
                    // Create new dot and add it to the form and list
                    PictureBox newEnemy = new PictureBox
                    {
                        Width = enemyInfo.Size,
                        Height = enemyInfo.Size,
                        Location = new Point(enemyInfo.X, enemyInfo.Y),
                        BackColor = Color.Black // Random color for variety
                    };

                    this.Controls.Add(newEnemy);
                    enemys.Add(newEnemy);
                }
            }

            //// Remove dots that no longer exist on the server
            //foreach (var existingDot in dots.ToList())
            //{
            //    // Create a unique identifier for the dot based on its position
            //    string existingDotId = $"{existingDot.Location.X}_{existingDot.Location.Y}_{existingDot.Width}";

            //    if (!dotIds.Contains(existingDotId))
            //    {
            //        // If the dot no longer exists on the server, remove it from the form and list
            //        this.Controls.Remove(existingDot);
            //        dots.Remove(existingDot);
            //    }
            //}
        }
        private void UpdateDots(List<DotInfo> serverDots)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    UpdateDots(serverDots);
                });
                return;
            }

            // Keep track of which dots from the server are new
            var dotIds = new HashSet<string>();

            // Update existing dots or add new ones if they don't exist
            foreach (var dotInfo in serverDots)
            {
                // Create a unique identifier for the dot based on its position
                string dotId = $"{dotInfo.X}_{dotInfo.Y}_{dotInfo.Size}";
                dotIds.Add(dotId);

                // Find if a dot with the same position and size already exists
                var existingDot = dots.FirstOrDefault(d =>
                    d.Location.X == dotInfo.X &&
                    d.Location.Y == dotInfo.Y &&
                    d.Width == dotInfo.Size &&
                    d.Height == dotInfo.Size);

                if (existingDot != null)
                {
                    // Dot already exists, just update its properties if needed (color, etc.)
                    //existingDot.BackColor = colors[random.Next(0, colors.Length)];
                }
                else
                {
                    // Create new dot and add it to the form and list
                    PictureBox newDot = new PictureBox
                    {
                        Width = dotInfo.Size,
                        Height = dotInfo.Size,
                        Location = new Point(dotInfo.X, dotInfo.Y),
                        BackColor = colors[random.Next(0, colors.Length)] // Random color for variety
                    };

                    this.Controls.Add(newDot);
                    dots.Add(newDot);
                }
            }

            // Remove dots that no longer exist on the server
            foreach (var existingDot in dots.ToList())
            {
                // Create a unique identifier for the dot based on its position
                string existingDotId = $"{existingDot.Location.X}_{existingDot.Location.Y}_{existingDot.Width}";

                if (!dotIds.Contains(existingDotId))
                {
                    // If the dot no longer exists on the server, remove it from the form and list
                    this.Controls.Remove(existingDot);
                    dots.Remove(existingDot);
                }
            }
        }

        private void UpdatePlayerStates(List<PlayerInfo> players)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    UpdatePlayerStates(players);
                });
                return;
            }

            // Update positions of existing players or create new ones if necessary
            foreach (PlayerInfo player in players)
            {
                if (player.Id == myPlayerId)
                {
                    pictureBox1.Width = player.Size;
                    pictureBox1.Height = player.Size;
                    // Skip the current player itself
                    continue;
                }

                // Check if the PictureBox for the player already exists
                if (otherPlayers.TryGetValue(player.Id, out PictureBox otherPlayer))
                {
                    // Update position of the existing PictureBox
                    otherPlayer.Location = new Point(player.X, player.Y);
                    otherPlayer.Size = new Size(player.Size, player.Size);
                }
                else
                {
                    // Create new PictureBox for the new player
                    otherPlayer = new PictureBox
                    {
                        Width = player.Size,
                        Height = player.Size,
                        Location = new Point(player.X, player.Y),
                        Margin = new Padding(3, 2, 3, 2),
                        Size = new Size(52, 45),

                        //BackColor = Color.Gray // Color for other players
                    };

                    RandomColor(otherPlayer);

                    otherPlayer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;

                    otherPlayer.SizeMode = PictureBoxSizeMode.StretchImage;

                    otherPlayers.Add(player.Id, otherPlayer);
                    this.Controls.Add(otherPlayer);
                }
            }

            // Remove any PictureBoxes for players that are no longer in the game
            var playerIds = new HashSet<Guid>(players.Select(p => p.Id));
            foreach (var kvp in otherPlayers.ToList())
            {
                if (!playerIds.Contains(kvp.Key))
                {
                    this.Controls.Remove(kvp.Value);
                    otherPlayers.Remove(kvp.Key);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

            
        }

        private void Expand(PictureBox obj, int amount)
        {
            obj.Width += amount;
            obj.Height += amount;
        }

        private void MoveToMouse(int x, int y, PictureBox obj, int speed)
        {
            // Get the x and y coordinates of the mouse pointer.
            //Point position = new Point(MousePosition.X, MousePosition.Y);
            //Point position = new Point(Cursor.Position.X, Cursor.Position.Y);
            Point position = new Point(x, y);
            Point newPosition = obj.Location;
            int pX = position.X - (obj.Width / 2);


            int pY = position.Y - (obj.Height / 2);
            position = new Point(pX, pY);
            //if (pX < 0) { pX = 0; }
            //if (pY < 0) { pY = 0; }
            //double xOffset = sender.ActualWidth / 2;
            //double yOffset = sender.ActualHeight / 2;
            //Canvas.SetLeft(sender, pX - xOffset);
            //Canvas.SetTop(sender, pY - yOffset);
            //moneyText.Content = pX + "X " + pY +  "Y";


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


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Point mouse = PointToClient(Cursor.Position);
            MoveToMouse(mouse.X, mouse.Y, pictureBox1, 3);

            // Broadcast player position and size to the server
            BroadcastInfo();

        }

        private void BroadcastInfo()
        {
            PlayerInfo playerData = new PlayerInfo
            {
                Id = myPlayerId,  // Ensure the player's unique ID is sent
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

        private void RandomColor(PictureBox obj)
        {

            int tempNum = random.Next(1, 7);
            switch (tempNum)
            {
                case 1:
                    obj.Image = Bitmap.FromFile("Violet.png");
                    //Bitmap bitmap0 = new Bitmap(obj.Image);
                    //var bt = MakeTransparent(bitmap0, Color.White, 100);
                    //pictureBox1.Image = bt;
                    break;
                case 2:
                    obj.Image = Bitmap.FromFile("Red.png");
                    //Bitmap bitmap1 = new Bitmap(obj.Image);
                    //MakeTransparent(bitmap1, Color.White, 100);
                    break;
                case 3:
                    obj.Image = Bitmap.FromFile("Yellow.png");
                    //Bitmap bitmap2 = new Bitmap(obj.Image);
                    //MakeTransparent(bitmap2, Color.White, 100);
                    break;
                case 4:
                    obj.Image = Bitmap.FromFile("Blue.png");
                    //Bitmap bitmap3 = new Bitmap(obj.Image);
                    //MakeTransparent(bitmap3, Color.White, 100);
                    break;
                case 5:
                    obj.Image = Bitmap.FromFile("Green.png");
                    //Bitmap bitmap4 = new Bitmap(obj.Image);
                    //MakeTransparent(bitmap4, Color.White, 100);
                    break;
                case 6:
                    obj.Image = Bitmap.FromFile("Orange.png");
                    //Bitmap bitmap5 = new Bitmap(obj.Image);
                    //MakeTransparent(bitmap5, Color.White, 100);
                    break;

                default:
                    obj.Image = Bitmap.FromFile("Violet.png");
                    //Bitmap bitmap6 = new Bitmap(obj.Image);
                    //MakeTransparent(bitmap6, Color.White, 100);
                    break;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }
    }
}