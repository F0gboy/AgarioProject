using Microsoft.VisualBasic.Devices;
using System.CodeDom.Compiler;

namespace AgarioProject
{
    public partial class Form1 : Form
    {
        public Random random = new Random();
        public List<PictureBox> dots = new List<PictureBox>();

        public int x;
        public int y;
        public int spawnTimer = 10;

        Color[] colors = { Color.AliceBlue, Color.Beige, Color.BlueViolet, Color.BurlyWood, Color.Crimson, Color.Cyan, Color.DarkGoldenrod, Color.DarkOrange, Color.DarkSeaGreen, Color.DeepPink, Color.HotPink, Color.LimeGreen };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //while (true)
            //{

            //}

            PictureBox asd = new PictureBox();
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

            if (newPosition.X + (pictureBox1.Width / 2) > 990)
            {
                newPosition.X = 990 - (pictureBox1.Width / 2);
            }

            if (newPosition.Y + (pictureBox1.Height / 2) > 950)
            {
                newPosition.Y = 950 - (pictureBox1.Height / 2);
            }

            if (newPosition.X + (pictureBox1.Width / 2) < 0)
            {
                newPosition.X = 0 - (pictureBox1.Width / 2);
            }

            if (newPosition.Y + (pictureBox1.Height / 2) < 0)
            {
                newPosition.Y = 0 - (pictureBox1.Height / 2);
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

            spawnTimer--;
            if (spawnTimer < 1)
            {
                SpawnDots();
                spawnTimer = 10;
            }

            foreach (PictureBox item in dots.ToList())
            {
                //x1 - x2 = x3
                //y1 - y2 = y3
                //Sqr(x3 ^ 2 + y3 ^ 2)


                int x3 = Math.Abs((pictureBox1.Location.X + (pictureBox1.Width / 2)) - item.Location.X);
                int y3 = Math.Abs((pictureBox1.Location.Y + (pictureBox1.Height / 2)) - item.Location.Y);

                double test = (Math.Pow(Math.Pow(x3, 2) + Math.Pow(y3, 2), 0.5f)) - (pictureBox1.Width / 2);

                if (test < -5)
                {
                    dots.Remove(item);
                    this.Controls.Remove(item);
                    Expand(pictureBox1, 1);
                }
            }

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
            PictureBox new_dot = new PictureBox();
            new_dot.Height = 10;
            new_dot.Width = 10;
            new_dot.BackColor = colors[random.Next(0, colors.Length)];

            x = random.Next(0, this.ClientSize.Width - new_dot.Width);
            y = random.Next(0, this.ClientSize.Height - new_dot.Height);

            new_dot.Location = new Point(x, y);

            dots.Add(new_dot);
            this.Controls.Add(new_dot);


        }
    }
}
