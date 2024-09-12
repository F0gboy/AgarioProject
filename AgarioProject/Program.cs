using System.Net.Sockets;

namespace AgarioProject
{

    internal static class Program
    {

        [STAThread]
        static void Main()
        {
            TcpClient client = new TcpClient();
            client.Connect("localhost", 12000);
            Console.WriteLine("Connected to server...");
            Thread receiveThread = new Thread(() => ReceiveMessages(client));
            receiveThread.Start();

           
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());

            StreamWriter writer = new StreamWriter(client.GetStream());
            while (true)
            {
                string message = Console.ReadLine();
                writer.WriteLine(message);
                writer.Flush();
            }

            void ReceiveMessages(TcpClient client)
            {
                StreamReader reader = new StreamReader(client.GetStream());
                while (client.Connected)
                {
                    string message = reader.ReadLine();
                    if (message != null)
                    {
                        Console.WriteLine(message);
                    }
                }
            }
        }
    }
}