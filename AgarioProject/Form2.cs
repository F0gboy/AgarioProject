using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace AgarioProject
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            string ipAddress = GetLocalIPAddress();
            form1.textBox1.Text = ipAddress;
            form1.ShowDialog();
        }
        private string GetLocalIPAddress()
        {
            string localIP = string.Empty;
            try
            {
                // Get the host entry for the local machine
                var host = Dns.GetHostEntry(Dns.GetHostName());

                // Loop through the address list and find the IPv4 address
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }

                // If no IPv4 address is found, return an error message
                if (string.IsNullOrEmpty(localIP))
                {
                    localIP = "No IPv4 address found.";
                }
            }
            catch (Exception ex)
            {
                localIP = $"Error: {ex.Message}";
            }

            return localIP;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();  // Hide Form2, but don't close it yet

            Form3 form3 = new Form3();
            form3.ShowDialog();  // Show Form3 modally

            this.Close();  // After Form3 is closed, close Form2

        }
    }
}
