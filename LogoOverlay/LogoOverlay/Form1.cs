﻿using MediaInfoNET;
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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogoOverlay
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
            try
            {
                string Json = "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim() + "/files/Logo/1000/" + System.Configuration.ConfigurationSettings.AppSettings["ServerCode"].Trim());

                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    Json = reader.ReadToEnd();
                }
                List<LogoQueue> ConvertList = JsonConvert.DeserializeObject<List<LogoQueue>>(Json);
                label2.Text = "Q:[" + ConvertList.Count + "]";
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                }
                label2.Text = "Q:[0]";
            }



        }
        protected void Convert()
        {
            string ErrorLog = "";
            try
            {
                QueueCount();
                string Json = "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim() + "/files/logo/1/" + System.Configuration.ConfigurationSettings.AppSettings["ServerCode"].Trim());

                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    Json = reader.ReadToEnd();
                }
                List<LogoQueue> ConvertList = JsonConvert.DeserializeObject<List<LogoQueue>>(Json);

                foreach (LogoQueue item in ConvertList)
                {
                    ClearDirectory(item.SrcDirectory);
                    ClearDirectory(item.ConvertDirectory);

                    progressBar1.Value = 0;
                    label1.Text = "0%";
                    richTextBox1.Text = "";

                    string SourceFile = item.SrcDirectory + item.Filename;
                    string[] SrcDir = SourceFile.Split('\\');
                    label3.Text = item.Filename;
                    if (System.IO.File.Exists(SourceFile))
                    {

                        MediaFile videof = new MediaFile(SourceFile);
                        long Duration = 0;
                        try
                        {
                            Duration = videof.Video[0].DurationMillis;
                            label5.Text = "Duration:" + videof.Video[0].DurationStringAccurate;
                        }
                        catch
                        {
                            label5.Text = "Duration Error";
                        }

                        string DestFile = item.SrcDirectory + "logo\\" + item.Filename;
                        string Command = "-i " + "\"" + SourceFile + "\""  + " -preset veryfast -vf \"movie=" + item.LogoFile + " [watermark]; [in][watermark] overlay=10:10 [out]\"    -y  " + "\"" + DestFile + "\"";
                        //if(item.Filename.ToLower().Contains(".mp3")|| item.Filename.ToLower().Contains(".wav"))
                        //{
                        //    Command = "-i " + "\"" + SourceFile + "\"" + " -vf \"movie=" + item.LogoFile + " [watermark]; [in][watermark] overlay=10:10 [out]\"    -y  " + "\"" + DestFile + "\"";
                        //}

                        if (!Directory.Exists(Path.GetDirectoryName(DestFile)))
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



                        bool Error = false;
                        if (item.LogoFile.Length > 0)
                        {
                            proc.Start();
                            StreamReader reader = proc.StandardError;
                            string line;

                            while ((line = reader.ReadLine()) != null)
                            {
                                if (richTextBox1.Lines.Length > 5)
                                {
                                    richTextBox1.Text = "";
                                }
                                if (line.ToLower().Contains("not found") || line.ToLower().Contains("invalid"))
                                {
                                    Error = true;
                                }

                                ErrorLog += line;
                                FindDuration(line);
                                richTextBox1.Text += (line) + " \n";
                                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                                richTextBox1.ScrollToCaret();
                                Application.DoEvents();
                            }
                        }
                        else
                        {
                            try
                            {
                                System.IO.File.Copy(SourceFile, DestFile, true);

                            }
                            catch (Exception er){
                                ErrorLog = "ERROR COPY" + er.Message;
                            }                  }

                        if (Error)
                        {
                            if (ErrorLog.Length > 500)
                            {
                                ErrorLog = ErrorLog.Substring(0, 499);
                            }
                            HttpWebRequest ReqError = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim() + "/files/Error?query=" + ErrorLog + "&id=" + item.FileId);
                            ReqError.GetResponse();
                        }
                        else
                        {
                            HttpWebRequest ReqDone = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim() + "/files/Logo/" + item.FileId + "/Done/" + Duration.ToString());
                            ReqDone.GetResponse();
                        }

                    }
                    else
                    {
                        richTextBox1.Text += SourceFile + " : Is not exist" + " \n";
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                        Application.DoEvents();
                        HttpWebRequest ReqError = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim() + "/files/Error?query=" + ErrorLog + "&id=" + item.FileId);
                        ReqError.GetResponse();

                    }
                }
            }
            catch (WebException ex)
            {
                //try
                //{
                //    WebResponse errorResponse = ex.Response;
                //    using (Stream responseStream = errorResponse.GetResponseStream())
                //    {
                //        StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                //        String errorText = reader.ReadToEnd();
                //    }
                //}
                //catch
                //{
                //}
                //HttpWebRequest ReqError = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim() + "/files/Error?query=" + ErrorLog + "&id=" + item.FileId);
                //ReqError.GetResponse();

            }




        }
        protected void FindDuration(string Str)
        {
            try
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
                    { }

                }
                if (Str.Contains("fps="))
                {
                    string Speed = "";
                    Speed = Str.Substring(Str.IndexOf("fps="), 8).Replace("fps=", "").Trim();
                    label4.Text = "Speed: " + (float.Parse(Speed) / 25).ToString() + " X ";
                    Application.DoEvents();
                }
            }
            catch (Exception exp)
            {
                richTextBox1.AppendText(exp.Message);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
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
                this.Height = 113;
            }
            else
            {
                this.Height = 248;
            }
        }
        private static void ClearDirectory(string startLocation)
        {
            int SavedH = 100;
            try
            {
                SavedH = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["SaveFilesHours"].Trim());
            }
            catch { }
            try
            {
                foreach (var directory in Directory.GetDirectories(startLocation))
                {
                    ClearDirectory(directory);
                    string[] Files = Directory.GetFiles(directory);
                    foreach (string item in Files)
                    {
                        if (System.IO.File.GetCreationTime(directory).AddHours(SavedH) < DateTime.Now)
                        {
                            try
                            {
                                System.IO.File.Delete(item);

                            }
                            catch { }                  }
                    }
                    if (Directory.GetCreationTime(directory).AddHours(SavedH) < DateTime.Now)
                    {
                        try
                        {
                            Directory.Delete(directory, false);

                        }
                        catch { }              }

                }
            }
            catch
            { }

        }
    }
    public class LogoQueue
    {
        public string ConvertDirectory { get; set; }
        public string SrcDirectory { get; set; }
        public string FileId { get; set; }
        public string Filename { get; set; }
        public string Logo { get; set; }
        public string LogoFile { get; set; }

    }

}
