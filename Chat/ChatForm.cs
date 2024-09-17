using System.Net.Sockets;

namespace Chat
{
    public partial class ChatForm : Form
    {
        public TcpClient client;
        private StreamWriter writer;
        private StreamReader reader;
        private Thread receiveThread;
        public ChatForm()
        {
            InitializeComponent();

            // Initialize and connect to the server
            client = new TcpClient();
            client.Connect("localhost", 12000);

            // Start a new thread to receive messages
            receiveThread = new Thread(() => ReceiveMessages(client));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            // Initialize the writer to send messages to the server
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

            reader = new StreamReader(client.GetStream());
            while (client.Connected)
            {
                try
                {
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
