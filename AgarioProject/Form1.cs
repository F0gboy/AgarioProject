using Microsoft.VisualBasic.Devices;
using System;
using System.CodeDom.Compiler;
using System.Net.Sockets;
using Newtonsoft.Json;
using Timer = System.Windows.Forms.Timer;
using System.Diagnostics;

namespace AgarioProject
{
    public partial class Form1 : Form
    {
        public Random random = new Random();
        public List<PictureBox> dots = new List<PictureBox>();
        public List<PictureBox> enemy = new List<PictureBox>();

        private Dictionary<Guid, PictureBox> otherPlayers = new Dictionary<Guid, PictureBox>();
        public int x;
        public int y;
        public int spawnTimer = 10;
        private Guid myPlayerId;
        private TcpClient client;
        private StreamWriter writer;
        private StreamReader reader;
        private Thread receiveThread;
        Color[] colors = { Color.AliceBlue, Color.Beige, Color.BlueViolet, Color.BurlyWood, Color.Crimson, Color.Cyan, Color.DarkGoldenrod, Color.DarkOrange, Color.DarkSeaGreen, Color.DeepPink, Color.HotPink, Color.LimeGreen };

        public class PlayerInfo
        {
            public Guid Id { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Size { get; set; }

            public PlayerInfo()
            {
                Id = Guid.NewGuid();
            }
        }

        public Form1()
        {
            InitializeComponent();
            try
            {
                client = new TcpClient();
                client.Connect("localhost", 12000);

                writer = new StreamWriter(client.GetStream());
                reader = new StreamReader(client.GetStream());

                InitializePlayer();
                StartSendingPlayerUpdates();

                Debug.WriteLine($"My player ID: {myPlayerId}");

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
                myPlayerId = Guid.NewGuid();
                Console.WriteLine($"Generated client ID: {myPlayerId}");
            }
        }

        private void StartSendingPlayerUpdates()
        {
            Timer sendTimer = new Timer();
            sendTimer.Interval = 100;
            sendTimer.Tick += (sender, e) => SendPlayerState();
            sendTimer.Start();
        }

        private void SendPlayerState()
        {
            if (client.Connected)
            {
                PlayerInfo myState = new PlayerInfo
                {
                    Id = myPlayerId,
                    X = pictureBox1.Location.X,
                    Y = pictureBox1.Location.Y,
                    Size = pictureBox1.Width
                };

                string myStateJson = JsonConvert.SerializeObject(myState);

                try
                {
                    writer.WriteLine(myStateJson);
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
                        List<PlayerInfo> allPlayers = JsonConvert.DeserializeObject<List<PlayerInfo>>(message);
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
            foreach (PlayerInfo player in allPlayers)
            {
                if (player.Id != myPlayerId)
                {
                    PictureBox otherPlayer = GetOrCreatePlayerPictureBox(player.Id);

                    otherPlayer.Location = new Point(player.X, player.Y);
                    otherPlayer.Width = player.Size;
                    otherPlayer.Height = player.Size;
                }
            }
        }

        private PictureBox GetOrCreatePlayerPictureBox(Guid playerId)
        {
            PictureBox playerBox = otherPlayers.ContainsKey(playerId) ? otherPlayers[playerId] : null;

            if (playerBox == null)
            {
                playerBox = new PictureBox
                {
                    BackColor = Color.Red
                };
                otherPlayers[playerId] = playerBox;
                this.Controls.Add(playerBox);
            }

            return playerBox;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //while (true)
            //{

            //}

            RandomColor(pictureBox1);
            
            
            for (int i = 0; i < 10; i++)
            {
                EnemySpawn();
            }            
        }

        private void Expand(PictureBox obj, int amount)
        {
            obj.Width += amount;
            obj.Height += amount;
        }

        private void Shrink(PictureBox obj, int amount)
        {
            obj.Width -= amount;
            obj.Height -= amount;
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

            if (newPosition.X + (obj.Width / 2) > 990)
            {
                newPosition.X = 990 - (obj.Width / 2);
            }

            if (newPosition.Y + (obj.Height / 2) > 950)
            {
                newPosition.Y = 950 - (obj.Height / 2);
            }

            if (newPosition.X + (obj.Width / 2) < 0)
            {
                newPosition.X = 0 - (obj.Width / 2);
            }

            if (newPosition.Y + (pictureBox1.Height / 2) < 0)
            {
                newPosition.Y = 0 - (pictureBox1.Height / 2);
            }

            obj.Location = newPosition;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Point mouse = PointToClient(Cursor.Position);
            MoveToMouse(mouse.X, mouse.Y, pictureBox1, 3);

            spawnTimer--;
            if (spawnTimer < 1)
            {
                SpawnDots();
                spawnTimer = 10;
            }

            foreach (PictureBox item in dots.ToList())
            {
                int x3 = Math.Abs((pictureBox1.Location.X + (pictureBox1.Width / 2)) - item.Location.X);
                int y3 = Math.Abs((pictureBox1.Location.Y + (pictureBox1.Height / 2)) - item.Location.Y);

                double distance = Math.Pow(Math.Pow(x3, 2) + Math.Pow(y3, 2), 0.5f) - (pictureBox1.Width / 2);

                if (distance < -5)
                {
                    dots.Remove(item);
                    this.Controls.Remove(item);
                    Expand(pictureBox1, 1);
                }
            }

            foreach (PictureBox ienemy in enemy.ToList())
            {
                int x3 = Math.Abs((pictureBox1.Location.X + (pictureBox1.Width / 2)) - ienemy.Location.X);
                int y3 = Math.Abs((pictureBox1.Location.Y + (pictureBox1.Height / 2)) - ienemy.Location.Y);

                double distance = Math.Pow(Math.Pow(x3, 2) + Math.Pow(y3, 2), 0.5f) - (pictureBox1.Width / 2);

                if (distance < -5)
                {
                    Shrink(pictureBox1, 10);
                }
            }
        }

        private void RandomColor(PictureBox obj)
        {
            int tempNum = random.Next(1, 7);
            switch (tempNum)
            {
                case 1:
                    obj.Image = Properties.Resources.Violet;
                    break;
                case 2:
                    obj.Image = Properties.Resources.Red;
                    break;
                case 3:
                    obj.Image = Properties.Resources.Yellow;
                    break;
                case 4:
                    obj.Image = Properties.Resources.Blue;
                    break;
                case 5:
                    obj.Image = Properties.Resources.Green;
                    break;
                case 6:
                    obj.Image = Properties.Resources.Orange;
                    break;
                default:
                    obj.Image = Properties.Resources.Blue;
                    break;
            }
        }

        private void SpawnDots()
        {
            PictureBox new_dot = new PictureBox
            {
                Height = 10,
                Width = 10,
                BackColor = colors[random.Next(0, colors.Length)],
                Location = new Point(random.Next(100, 800), random.Next(100, 800))
            };
            this.Controls.Add(new_dot);
            dots.Add(new_dot);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            // Code for handling mouse movement within the form
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Code for handling the click event
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // Your custom drawing logic here
        }



    }
}
