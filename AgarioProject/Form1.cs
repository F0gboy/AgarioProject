using Microsoft.VisualBasic.Devices;

namespace AgarioProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //while (true)
            //{

            //}

          
        }



        private void MoveToMouse(int x, int y, PictureBox obj)
        {
            // Get the x and y coordinates of the mouse pointer.
            //Point position = new Point(MousePosition.X, MousePosition.Y);
            //Point position = new Point(Cursor.Position.X, Cursor.Position.Y);
            Point position = new Point(x, y);
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


            obj.Location = position;

        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {


        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            Point mouse = PointToClient(Cursor.Position);
            MoveToMouse(mouse.X, mouse.Y, pictureBox1);
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
                        pictureBox1.Width / 2 - (TextRenderer.MeasureText(pictureBox1.Width.ToString(), myFont).Width / 2)+3,
                        pictureBox1.Height / 2 - (TextRenderer.MeasureText(pictureBox1.Width.ToString(), myFont).Height / 2)));
            }
        }
    }
}
