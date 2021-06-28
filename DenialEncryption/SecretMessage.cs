using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DenialEncryption
{
    public partial class SecretMessage : Form
    {
        public SecretMessage(string message)
        {
            InitializeComponent();
            textBox1.Text = message;
            textBox1.ReadOnly = false;

        }
    }
}
