using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace AgarioProject
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        // Form3.cs
        private void button1_Click(object sender, EventArgs e)
        {
            string ipAddress = textBox1.Text;  // Get the IP address from the TextBox

            if (string.IsNullOrEmpty(ipAddress))
            {
                MessageBox.Show("Please enter a valid IP address.");
                return;
            }

            this.Hide();
            Form1 form1 = new Form1();
            ChatForm chatForm = new ChatForm(ipAddress);  // Pass the IP to ChatForm
            form1.Show();
            chatForm.Show();
         

            this.Close();
        }


        //private void button1_Click(object sender, EventArgs e)
        //{
        //   // SaveIPAddress();

        //    this.Hide();
        //    Form1 form1 = new Form1();
        //    string ipAddress = textBox1.Text;  // Get the IP address from the TextBox
        //    ChatForm chatForm = new ChatForm(ipAddress);  // Pass the IP to ChatForm
        //    //ChatForm chatForm = new ChatForm();
        //    chatForm.Show();
        //    form1.Show();
        //    this.Close();
        //}

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }


        public void SaveIPAddress()
        {
            string ipAddress = textBox1.Text;  // Get IP from TextBox
            //File.WriteAllText("ip_address.txt", ipAddress);  // Save to a file

            
        }

    }
}
