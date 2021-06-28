using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DenialEncryption
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                var messages = int.Parse(textBox1.Text);
                var form2 = new Form1(messages);
                form2.ShowDialog();

            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var form = new Form3();
            form.ShowDialog();
        }
    }
}
