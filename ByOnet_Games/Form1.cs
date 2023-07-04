using System;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Net;
using System.Text;
using System.IO.Compression;
using System.Diagnostics;

namespace ByOnet_Games
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string version = "";
        public string serverVersion = "";
        public string path = @"Game\";
        public string exename = "Game";
        public string url = "http://192.168.1.60/ByOnet/";
        public bool downloading = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory(Path.GetFullPath(path));
            if (!File.Exists(Path.GetFullPath(path + exename)))
            {
                RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey("Shooter");
                key.SetValue("version", "none");
                key.Close();
                version = "none";
            }
            else
            {
                version = (string)Registry.CurrentUser.OpenSubKey("Shooter").GetValue("version");
            }


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + "v.txt");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream recivestream = response.GetResponseStream();
                StreamReader readstream = null;
                if (response.CharacterSet == "null")
                {
                    readstream = new StreamReader(recivestream);
                }
                else
                {
                    readstream = new StreamReader(recivestream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readstream.ReadToEnd();
                serverVersion = data;
                response.Close();
                readstream.Close();
            }
            else
            {
                MessageBox.Show("Ошибка соединения с сервером", "ByOnet");
            }

            label1.Text = version + "/" + serverVersion;

            if (version != serverVersion)
            {
                if (File.Exists(Path.GetFullPath(path + exename)))
                {
                    button1.Text = "Обновить";
                    button1.Click += UpdateGame;
                }
                else
                {
                    button1.Text = "Скачать";
                    button1.Click += UpdateGame;
                }
            }
            else
            {
                button1.Text = "Запуск";
                button1.Click += Play;
            }
        }

        private void UpdateGame(object sender, EventArgs e)
        {
            downloading = false;
            button1.Enabled = false;
            DirectoryInfo dir = new DirectoryInfo(Path.GetFullPath(path));
            foreach (FileInfo item in dir.GetFiles())
            {
                item.Delete();
            }
            foreach (DirectoryInfo item in dir.GetDirectories())
            {
                item.Delete(true);
            }


            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += (s, g) =>
                {
                    if (g.ProgressPercentage == 100)
                    {
                        if (!File.Exists(Path.GetFullPath(path + exename)))
                        {
                            ZipFile.ExtractToDirectory("g.zip", path);
                            RegistryKey key;
                            key = Registry.CurrentUser.CreateSubKey("Shooter");
                            key.SetValue("version", serverVersion);
                            key.Close();
                            version = serverVersion;
                            label1.Text = version + "/" + serverVersion;

                            button1.Click -= UpdateGame;
                            button1.Click += Play;
                        }
                        button1.Enabled = true;
                        downloading = false;
                    }
                };

                wc.DownloadFileAsync(new Uri(url + "Game.zip"), "g.zip");
            }
        }

        private void Play(object sender, EventArgs e)
        {
            Process.Start(path + exename);
        }
    }
}
