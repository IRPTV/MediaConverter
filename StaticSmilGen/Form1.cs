using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StaticSmilGen
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        protected void Logger(string txt)
        {
            if (richTextBox1.Lines.Length > 500)
            {
                richTextBox1.Text = "";
            }
            richTextBox1.Text += "[" + DateTime.Now.ToString() + "] " + (txt) + " \n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
            Application.DoEvents();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DoMain(@"E:\vod\ptv");
        }
        int count = 0;
        protected void DoMain(string startLocation)
        {
            try
            {
                foreach (var directory in Directory.GetDirectories(startLocation))
                {
                    DoMain(directory);
                    string[] Files = Directory.GetFiles(directory);
                    foreach (string item in Files)
                    {
                        if (!item.ToLower().Contains("_low") && item.ToLower().Contains(".mp4"))
                        {
                            count++;
                            label1.Text = count.ToString();
                            Logger(item);
                            string aa = System.IO.File.ReadAllText(Path.GetDirectoryName(Application.ExecutablePath) + "\\template.xml");
                            DateTime dt = DateTime.Parse("1/1/1");
                            Logger("START SMIL:");
                            aa = aa.Replace("{{FILENAME}}", Path.GetFileNameWithoutExtension(item));
                            Logger(aa);
                            System.IO.File.WriteAllText(item.Replace(".mp4",".smil"), aa);
                        }
                    }
                }
            }
            catch
            { }
        }

    }
}