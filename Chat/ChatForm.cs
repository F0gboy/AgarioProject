using System.Net.Sockets;

namespace Chat
{
    public partial class ChatForm : Form
    {
        public TcpClient client;
        private StreamWriter writer;
        private StreamReader? reader;
        private Thread receiveThread;
        private string hostname;
        public ChatForm()
        {
            InitializeComponent();
            LoadIPAddress();
            // Initialize and connect to the server
            //client = new TcpClient();
            ////client.Connect("localhost", 12000);

            //// Start a new thread to receive messages
            //receiveThread = new Thread(() => ReceiveMessages(client));
            //receiveThread.IsBackground = true;
            //receiveThread.Start();

            //// Initialize the writer to send messages to the server
            //writer = new StreamWriter(client.GetStream());
        }
        public void LoadIPAddress()
        {
            string ipAddress = "localhost";  // Default IP address

            if (File.Exists("ip_address.txt"))
            {
                try
                {
                    ipAddress = File.ReadAllText("ip_address.txt").Trim();  // Read and trim IP from file
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading IP address from file: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("IP address file not found. Using default IP 'localhost'.");
            }

            client = new TcpClient();
            try
            {
                client.Connect(ipAddress, 12000);  // Use the IP address to connect
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to the server: {ex.Message}");
            }

            receiveThread = new Thread(() => ReceiveMessages(client));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            writer = new StreamWriter(client.GetStream());


        }

        private void button1_Click(object sender, EventArgs e)
        {
            string messageToSend = richTextBox2.Text;
            if (!string.IsNullOrWhiteSpace(messageToSend))
            {
                writer.WriteLine(messageToSend);
                writer.Flush();

                // Clear the input box after sending the message
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
                        // Update UI in a thread-safe way using Invoke
                        Invoke((MethodInvoker)delegate
                        {
                            richTextBox1.AppendText(message + Environment.NewLine);
                        });
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

        }
    }
}
