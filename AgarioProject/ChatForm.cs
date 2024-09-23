using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgarioProject
{
    public partial class ChatForm : Form
    {
        public TcpClient client;
        private StreamWriter writer;
        private StreamReader? reader;
        private Thread receiveThread;

        public ChatForm(string hostname)
        {
            InitializeComponent();
            Console.WriteLine($"Connecting to server at: {hostname}");

            // Initialize and connect to the server
            client = new TcpClient();
            try
            {
                client.Connect(hostname, 12000);
                writer = new StreamWriter(client.GetStream());

                // Start a new thread to receive messages
                receiveThread = new Thread(() => ReceiveMessages(client));
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Could not connect to server at {hostname}. Error: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string messageToSend = richTextBox2.Text;
            if (!string.IsNullOrWhiteSpace(messageToSend))
            {
                writer.WriteLine(messageToSend);
                writer.Flush();
                richTextBox2.Clear();
            }
        }

        private void ReceiveMessages(TcpClient client)
        {
            richTextBox1.ReadOnly = true;

            while (client.Connected)
            {
                try
                {
                    reader = new StreamReader(client.GetStream());
                    string message = reader.ReadLine();
                    if (message != null)
                    {
                        // Check if the handle is created before invoking
                        if (IsHandleCreated)
                        {
                            BeginInvoke((MethodInvoker)delegate
                            {
                                richTextBox1.AppendText(message + Environment.NewLine);
                            });
                        }
                    }
                }
                catch (IOException)
                {
                    // Handle disconnection or other I/O issues
                    break;
                }
            }
        }

        private void ChatForm_Load(object sender, EventArgs e)
        {
            // Any additional logic when form loads
        }
    }
}


//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;

//namespace AgarioProject
//{
//    public partial class ChatForm : Form
//    {
//        public TcpClient client;
//        private StreamWriter writer;
//        private StreamReader? reader;
//        private Thread receiveThread;
//        public ChatForm(string hostname)
//        {



//                InitializeComponent();

//                // Print IP address to console or log for debugging
//                Console.WriteLine($"Connecting to server at: {hostname}");

//                // Initialize and connect to the server
//                client = new TcpClient();
//                try
//                {
//                    client.Connect(hostname, 12000);
//                    // Initialize the writer to send messages to the server
//                    writer = new StreamWriter(client.GetStream());

//                    // Start a new thread to receive messages
//                    receiveThread = new Thread(() => ReceiveMessages(client));
//                    receiveThread.IsBackground = true;
//                    receiveThread.Start();
//                }
//                catch (SocketException ex)
//                {
//                    MessageBox.Show($"Could not connect to server at {hostname}. Error: {ex.Message}");
//                }




//            // InitializeComponent();

//            // // Initialize and connect to the server
//            // client = new TcpClient();
//            //// client.Connect("localhost", 12000);
//            // //string hostname;
//            // Form3 form3= new Form3();
//            // hostname = form3.textBox1.Text;
//            // client.Connect(hostname, 12000);
//            // //// Start a new thread to receive messages
//            // receiveThread = new Thread(() => ReceiveMessages(client));
//            // receiveThread.IsBackground = true;
//            // receiveThread.Start();

//            // //// Initialize the writer to send messages to the server
//            // writer = new StreamWriter(client.GetStream());
//        }


//        private void button1_Click(object sender, EventArgs e)
//        {
//            string messageToSend = richTextBox2.Text;
//            if (!string.IsNullOrWhiteSpace(messageToSend))
//            {
//                writer.WriteLine(messageToSend);
//                writer.Flush();

//                // Clear the input box after sending the message
//                richTextBox2.Clear();
//            }

//        }
//        private void ReceiveMessages(TcpClient client)
//        {
//            richTextBox1.ReadOnly = true;

//            while (client.Connected)
//            {
//                try
//                {
//                    reader = new StreamReader(client.GetStream());
//                    string message = reader.ReadLine();
//                    if (message != null)
//                    {
//                        // Update UI in a thread-safe way using Invoke
//                        Invoke((MethodInvoker)delegate
//                        {
//                            richTextBox1.AppendText(message + Environment.NewLine);
//                        });
//                    }
//                }
//                catch (IOException)
//                {
//                    // Handle disconnection or other I/O issues
//                    break;
//                }
//            }
//        }


//        private void ChatForm_Load(object sender, EventArgs e)
//        {

//        }
//    }
//}
