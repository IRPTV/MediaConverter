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
        protected void QueueCount()
        {
            string ProfId = System.Configuration.ConfigurationSettings.AppSettings["ProfileId"].Trim();
            string Json = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://192.168.10.26/mc.svc/files/convert/1000/false/" + ProfId);
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
            label2.Text = "Queue: ["+ConvertList.Count+"] Files";
        }
        protected void Convert()
        {
            QueueCount();
            string ProfId = System.Configuration.ConfigurationSettings.AppSettings["ProfileId"].Trim();
            string Json = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://192.168.10.26/mc.svc/files/convert/1/false/" + ProfId);
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

                string SourceFile = item.SrcDirectory+"logo\\" + item.Filename;
                string[] SrcDir = SourceFile.Split('\\');
               
                if (File.Exists(SourceFile))
                {
                    string DestFile = item.ConvertDirectory +SrcDir[SrcDir.Length-2]+"\\"+ Path.GetFileNameWithoutExtension(SourceFile)+ item.FilenameSuffix;
                    string Command = item.Command.Replace("{in}", SourceFile).Replace("{out}", DestFile);

                    if(!Directory.Exists(Path.GetDirectoryName(DestFile)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(DestFile));
                    }

                    Process proc = new Process(); if (Environment.Is64BitOperatingSystem)
                    {
                        proc.StartInfo.FileName = Path.GetDirectoryName(Application.ExecutablePath) + "\\ffmpeg64";
                    }
                    else
                    {
                        proc.StartInfo.FileName = Path.GetDirectoryName(Application.ExecutablePath) + "\\ffmpeg32";
                    }
                    proc.StartInfo.Arguments = Command;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.EnableRaisingEvents = true;

                    proc.Start();

                    HttpWebRequest ReqStart = (HttpWebRequest)WebRequest.Create("http://192.168.10.26/mc.svc/files/convert/" + item.ConvertId + "/start");
                    ReqStart.GetResponse();

                    StreamReader reader = proc.StandardError;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (richTextBox1.Lines.Length > 5)
                        {
                            richTextBox1.Text = "";
                        }

                        FindDuration(line);
                        richTextBox1.Text += (line) + " \n";
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                        Application.DoEvents();
                    }
                    HttpWebRequest ReqDone = (HttpWebRequest)WebRequest.Create("http://192.168.10.26/mc.svc/files/convert/" + item.ConvertId + "/done");
                    ReqDone.GetResponse();


                    progressBar1.Value = progressBar1.Maximum;
                    label1.Text = "100%";
                }
                else
                {
                    richTextBox1.Text += SourceFile + " : Is not exist" + " \n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    Application.DoEvents();

                }
            }
        }
        protected void FindDuration(string Str)
        {
            string TimeCode = "";
            if (Str.Contains("Duration:"))
            {
                TimeCode = Str.Substring(Str.IndexOf("Duration: "), 21).Replace("Duration: ", "").Trim();
                string[] Times = TimeCode.Split('.')[0].Split(':');
                double Frames = double.Parse(Times[0].ToString()) * (3600) * (25) +
                    double.Parse(Times[1].ToString()) * (60) * (25) +
                    double.Parse(Times[2].ToString()) * (25);
                progressBar1.Maximum = int.Parse(Frames.ToString());
            }
            if (Str.Contains("time="))
            {
                try
                {
                    string CurTime = "";
                    CurTime = Str.Substring(Str.IndexOf("time="), 16).Replace("time=", "").Trim();
                    string[] CTimes = CurTime.Split('.')[0].Split(':');
                    double CurFrame = double.Parse(CTimes[0].ToString()) * (3600) * (25) +
                        double.Parse(CTimes[1].ToString()) * (60) * (25) +
                        double.Parse(CTimes[2].ToString()) * (25);


                    progressBar1.Value = int.Parse(CurFrame.ToString());

                    label1.Text = ((progressBar1.Value * 100) / progressBar1.Maximum).ToString() + "%";

                    Application.DoEvents();
                }
                catch
                {}

            }
            if (Str.Contains("fps="))
            {
                string Speed = "";
                Speed = Str.Substring(Str.IndexOf("fps="), 8).Replace("fps=", "").Trim();
                label4.Text = "Speed: " + (float.Parse(Speed) / 25).ToString() + " X ";
                Application.DoEvents();
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
          label3.Text=  System.Configuration.ConfigurationSettings.AppSettings["ProfileTitle"].Trim();
            timer1.Enabled = true;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            btnStart_Click(null, null);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(this.Height==248)
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