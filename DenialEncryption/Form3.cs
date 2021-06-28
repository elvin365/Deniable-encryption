using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace DenialEncryption
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }
        private void button2_Click(object sender, EventArgs e)
        {
           if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                 string name = openFileDialog1.FileName;
                textBox1.Text = File.ReadAllText(name);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using StreamReader r = new StreamReader(string.Concat(System.Environment.CurrentDirectory, "\\messages.txt"));
            string json = r.ReadToEnd();
            var temp = JObject.Parse(json);
            string test;
            var message = GetMessages(temp);

            if (message.Text != null)
            {
                var exist = message.ListMessages.Exists(m => m.Key == textBox2.Text);
                if (!string.IsNullOrEmpty(temp.ToString()))
                {
                    if (message.ListMessages.Count != 0)
                    {
                        test = message.ListMessages.FirstOrDefault(m => m.Key == textBox2.Text).Text;
                        var result = EncryptHelper.Decrypt(test, textBox2.Text);
                        MessageBox.Show(result);
                    }
                    else
                        MessageBox.Show("Wow...you were tricked");
                }
            }
            else
                MessageBox.Show("Wow...you were tricked");
        }
        public Messages GetMessages(JObject json)
        {
            var message = new Messages();
            var actual = false;
            foreach (var mes in json)
            {
                if (actual)
                {
                    var test = mes.Value.ToList();
                    message.ListMessages = new List<Message>();
                    
                    foreach (var prueba in test)
                    {
                        var jsonA = JsonConvert.DeserializeObject<Message>(prueba.ToString());
                        if (jsonA.Key != textBox2.Text)
                            continue;
                        message.ListMessages.Add(jsonA);
                        actual = false;
                        break;
                    }
                }
                if (string.Equals(mes.Value.ToString(), textBox1.Text))
                {
                    message.Text = mes.Value.ToString();
                    actual = true;
                }
            }
            return message;
        }
    }
}
