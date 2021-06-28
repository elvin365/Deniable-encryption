using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;

namespace DenialEncryption
{
    public partial class Form1 : Form
    {
        int Messages = 0;
        int Times;
        public Form1(int times)
        {
            Times = times;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            save.newData(textBox1.Text, textBox2.Text);
            textBox1.Text = "";
            textBox2.Text = "";
            Messages++;

            if (Messages == Times)
            {
                this.Close();
                save.write();
            }                    
        }
    }

    public class Messages
    {
        [JsonProperty("Text")]
        public string Text { get; set; }
        [JsonProperty("ListMessages")]
        public List<Message> ListMessages { get; set; }
    }
    class Wrapper
    {
        public Messages Messages { get; set; }
    }
    public class Message
    {
        [JsonProperty("Text")]
        public string Text { get; set; }
        [JsonProperty("Key")]
        public string Key { get; set; }
    }

    public class save
    {
        static Message _messages = new Message();
        static Messages Messages = new Messages();
        public static void saveText(string key, string text)
        {
            _messages = new Message
            {
                Key = key,
                Text = text
            };
            if(Messages.ListMessages == null)
                Messages.ListMessages = new List<Message>();
            Messages.ListMessages.Add(_messages);

        }
        public static void newData(string key, string text)
        {
            var newDa = new Message
            {
                Key = key,
                Text = EncryptHelper.Encrypt(text, key)
            };

            saveText(newDa.Key, newDa.Text);
        }

        public static void write()
        {
            string text = "";
            foreach (var mes in Messages.ListMessages)
            {
                text = string.Concat(text, mes.Text);
            }
            //MD5 md5 = new MD5CryptoServiceProvider();
            //md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));
            //byte[] result = md5.Hash;
            SHA256 sh = SHA256.Create();
            sh.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));
            byte[] result = sh.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                strBuilder.Append(result[i].ToString("x2"));
            }

            text = strBuilder.ToString();

            Messages.Text = text;
            var json = System.Text.Json.JsonSerializer.Serialize(Messages);
            var form = new SecretMessage(text);
            form.ShowDialog();
            File.WriteAllText(string.Concat(System.Environment.CurrentDirectory, "\\messages.txt"), json);


            //-----------------------------
            string dirWork = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string dbPath = dirWork + @"\" + "Cip" + ".db"; // задаём путь и мя базы данных
            string dbTableName = "text"; // задаём имя таблицы, с которой будем работать
            if (!File.Exists(dbPath) & dbPath != null) // если базы данных нету, то...
            {
                SQLiteConnection.CreateFile(dbPath); // создать базу данных
                MessageBox.Show("База данных создана");
            }
            else
            {
                // ДОПИСАТЬ
                SQLiteConnection Connect = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;");
                string commandText = "DROP Table " + dbTableName; //  её нет
                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
                //MessageBox.Show("База данных удалена");

            }

            using (SQLiteConnection Connect = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;")) // в строке указывается к какой базе подключаемся
            {
                // строка запроса, который надо будет выполнить на базе
                string commandText = "CREATE TABLE IF NOT EXISTS " + dbTableName + "( [id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, [file] BINARY, [file_format] VARCHAR(10), [file_name] NVARCHAR(128))"; // создать таблицу, если её нет
                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                Connect.Open();
                int _result = Command.ExecuteNonQuery(); // _result для if-else
                // или этот код: Command.ExecuteNonQuery();
                Connect.Close();

                // ниже необязательный код if-else
                if (_result == 0) // если таблица создана, то...
                {
                    MessageBox.Show("Таблица создана");
                }
                else
                {
                    MessageBox.Show("Таблица '" + dbTableName + "' в базе данных уже существует");
                }
            }
            string filePath = System.Environment.CurrentDirectory + "\\messages.txt";
            string fileFormat = "txt";
            string fileName = "messages";

            // Конвертируем файл в байты byte[]
            byte[] _fileBytes = null;
            FileInfo _fileInfo = new FileInfo(filePath); // загрузим файл
            long _numBytes = _fileInfo.Length; // вычислим длину
            FileStream _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read); // откроем файл на чтение
            BinaryReader _binReader = new BinaryReader(_fileStream);
            _fileBytes = _binReader.ReadBytes((int)_numBytes); // файл в байтах

            //fileFormat = Path.GetExtension(filePath).Replace(".", "").ToLower(); // запишем в переменную расширение файла в нижнем регистре, не забыв удалить (Replace) точку перед расширением
            //fileName = Path.GetFileName(openFileDialogFile.FileName).Replace(Path.GetExtension(filePath), ""); // запишем в переменную имя файла, не забыв удалить (Replace) расширение с точкой

            // записываем информацию в базу данных
            using (SQLiteConnection Connect = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;"))
            {
                string commandText = "INSERT INTO " + dbTableName + " ([file], [file_format], [file_name]) VALUES(@file, @format, @name)";
                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@file", _fileBytes);
                Command.Parameters.AddWithValue("@format", fileFormat);
                Command.Parameters.AddWithValue("@name", fileName);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
                MessageBox.Show("Шифр добавлен в базу данных");
            }




        }
    }

    public static class EncryptHelper
    {
        public static string Encrypt(string clearText, string key)
        {
            string EncryptionKey = key;
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(string cipherText, string key)
        {
            string EncryptionKey = key;
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
