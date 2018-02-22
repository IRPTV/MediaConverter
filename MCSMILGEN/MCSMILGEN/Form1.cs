using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace MCConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            btnStart.ForeColor = Color.White;
            btnStart.Text = "Started";
            btnStart.BackColor = Color.Red;

            Convert();

            btnStart.ForeColor = Color.White;
            btnStart.Text = "Start";
            btnStart.BackColor = Color.Navy;
            timer1.Enabled = true;

        }
        protected void Logger(string txt)
        {
            if (richTextBox1.Lines.Length > 500)
            {
                richTextBox1.Text = "";
            }                      
            richTextBox1.Text += "["+DateTime.Now.ToString()+"] "+(txt) + " \n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
            Application.DoEvents();
        }
        protected void QueueCount()
        {
            try
            {
                string ProfId = System.Configuration.ConfigurationSettings.AppSettings["ProfileId"].Trim();
                string Json = "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim() + "/files/convert/1000/false/" + ProfId +
                    "/" + System.Configuration.ConfigurationSettings.AppSettings["ServerCode"].Trim());
                Logger("Q count:"+ request.RequestUri.ToString());
                try
                {
                    WebResponse response = request.GetResponse();
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        Json = reader.ReadToEnd();
                    }
                }
                catch (WebException ex)
                {
                    WebResponse errorResponse = ex.Response;
                    using (Stream responseStream = errorResponse.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                        String errorText = reader.ReadToEnd();
                    }
                }


                List<ConvertQueue> ConvertList = JsonConvert.DeserializeObject<List<ConvertQueue>>(Json);
                Logger("Q count:"+ ConvertList.Count);
                label2.Text = "Q:[" + ConvertList.Count + "]";
            }
            catch (Exception EXp)
            {
                richTextBox1.Text += (EXp) + " \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }
        }
        protected void Convert()
        {
            try
            {
                QueueCount();
                string ProfId = System.Configuration.ConfigurationSettings.AppSettings["ProfileId"].Trim();
                string Json = "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim() + "/files/convert/1/false/" + ProfId + "/" + System.Configuration.ConfigurationSettings.AppSettings["ServerCode"].Trim());
                try
                {
                    WebResponse response = request.GetResponse();
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        Json = reader.ReadToEnd();
                    }
                }
                catch (WebException ex)
                {
                    WebResponse errorResponse = ex.Response;
                    using (Stream responseStream = errorResponse.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                        String errorText = reader.ReadToEnd();
                    }
                }

                List<ConvertQueue> ConvertList = JsonConvert.DeserializeObject<List<ConvertQueue>>(Json);

                foreach (ConvertQueue item in ConvertList)
                {
                    progressBar1.Value = 0;
                    label1.Text = "0%";
                    richTextBox1.Text = "";
                    label5.Text = item.Filename;

                    string SourceFile = item.SrcDirectory + "logo\\" + item.Filename;
                    Logger("Source:"+ SourceFile);
                    string[] SrcDir = SourceFile.Split('\\');

                    if (File.Exists(SourceFile))
                    {
                        try
                        {
                            HttpWebRequest ReqStart = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim() + "/files/convert/" + item.ConvertId + "/start");
                            ReqStart.GetResponse();
                        }
                        catch { }
                                     
                        string DestFile = item.ConvertDirectory + SrcDir[SrcDir.Length - 2] + "\\" + Path.GetFileNameWithoutExtension(SourceFile) + item.FilenameSuffix;
                        Logger("Dest:" + DestFile);
                        if (!Directory.Exists(Path.GetDirectoryName(DestFile)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(DestFile));
                        }
                        string aa = System.IO.File.ReadAllText(Path.GetDirectoryName(Application.ExecutablePath) + "\\template.xml");
                        DateTime dt = DateTime.Parse("1/1/1");
                        Logger("START SMIL:");
                        aa = aa.Replace("{{FILENAME}}", Path.GetFileNameWithoutExtension(SourceFile));
                        Logger(aa);
                        System.IO.File.WriteAllText(DestFile, aa);
                        Logger("File Saved");
                        HttpWebRequest ReqDone = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim() + "/files/convert/" + item.ConvertId + "/done");
                        if (File.Exists(DestFile))
                        {
                            ReqDone.GetResponse();
                        }

                        if (DestFile.Contains(".jpg") || DestFile.Contains(".png"))
                        {
                            ReqDone.GetResponse();
                        }
                        progressBar1.Value = progressBar1.Maximum;
                        label1.Text = "100%";
                        
                    }
                    else
                    {
                        Logger("FILE NOT EXIST");
                        HttpWebRequest ReqDelete = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim() + "/files/convert/" + item.ConvertId + "/delete");
                        ReqDelete.GetResponse();
                        
                        richTextBox1.Text += SourceFile + " : Is not exist" + " \n";
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                        Application.DoEvents();


                    }
                }
            }
            catch (Exception Exp)
            {
                timer1.Enabled = true;
                richTextBox1.Text += Exp;
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }
        }
       
        private void Form1_Load(object sender, EventArgs e)
        {
            label3.Text = System.Configuration.ConfigurationSettings.AppSettings["ProfileTitle"].Trim();
            timer1.Enabled = true;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            btnStart_Click(null, null);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (this.Height == 248)
            {
                this.Height = 103;
            }
            else
            {
                this.Height = 248;
            }
        }
    }
    public class ConvertQueue
    {
        public string ConvertId { get; set; }
        public string ConvertDirectory { get; set; }
        public string SrcDirectory { get; set; }
        public string FileId { get; set; }
        public string Filename { get; set; }
        public string Command { get; set; }
        public string FilenameSuffix { get; set; }
        public string ProfileKind { get; set; }


    }
}