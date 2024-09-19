using System.Drawing.Text;
using System.Net.Sockets;

using static System.Net.Mime.MediaTypeNames;
namespace Chat
{
    public partial class ChatForm: Form
    {
        
       


            /// <summary>
            ///  Required designer variable.
            /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            richTextBox1 = new RichTextBox();
            richTextBox2 = new RichTextBox();
            button1 = new Button();
            SuspendLayout();
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(12, 27);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(356, 227);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // richTextBox2
            // 
            richTextBox2.Location = new Point(12, 297);
            richTextBox2.Name = "richTextBox2";
            richTextBox2.Size = new Size(356, 143);
            richTextBox2.TabIndex = 1;
            richTextBox2.Text = "";
            // 
            // button1
            // 
            button1.Location = new Point(101, 492);
            button1.Name = "button1";
            button1.Size = new Size(189, 55);
            button1.TabIndex = 2;
            button1.Text = "Send";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // ChatForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(393, 577);
            Controls.Add(button1);
            Controls.Add(richTextBox2);
            Controls.Add(richTextBox1);
            Name = "ChatForm";
            Text = "Chat";
            Load += ChatForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox richTextBox1;
        private RichTextBox richTextBox2;
        private Button button1;
    }
}
