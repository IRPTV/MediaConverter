﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace McUploader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //timer1.Enabled = true;
        }
        protected void ProccessList()
        {

            try
            {
                int Ttl = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["Count"].Trim());
                string DestSrv = System.Configuration.ConfigurationSettings.AppSettings["DestServer"].Trim();
                label2.Text = DestSrv;
                string Json = "";
                // HttpWebRequest request = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Server"].Trim() + "/mc.svc/files/upload/" + Ttl);


                WebRequest request = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Server"].Trim() + "/mc.svc/files/upload/" + Ttl + "/" + System.Configuration.ConfigurationSettings.AppSettings["ServerCode"].Trim());
                //request.Timeout = Timeout.Infinite;
                //request. = true;
                // try
                // {
                //try
                //{
                //    WebResponse response = request.GetResponse();
                //    using (Stream responseStream = response.GetResponseStream())
                //    {
                //        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                //        Json = reader.ReadToEnd();
                //    }
                //}
                //catch (WebException ex)
                //{
                //    WebResponse errorResponse = ex.Response;
                //    using (Stream responseStream = errorResponse.GetResponseStream())
                //    {
                //        StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                //        String errorText = reader.ReadToEnd();
                //    }
                //}

                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    Json = reader.ReadToEnd();
                    reader.Close();
                    responseStream.Close();

                }
                response.Close();

                //}
                //catch (Exception Err)
                //{
                //    LogWriter(Err.Message);
                //    // return;
                //}



                List<UploadQueue> ConvertList = JsonConvert.DeserializeObject<List<UploadQueue>>(Json);

                label3.Text = DateTime.Now.ToString();
                dataGridView1.Rows.Clear();
                int Qcount = 0;
                if (ConvertList.Count > 0)
                {
                    UploadQueue Itm = new UploadQueue();
                    bool ItemFound = false;
                    foreach (UploadQueue Itm2 in ConvertList)
                    {
                        if (Itm2.ServerIp.ToLower().Contains(DestSrv))
                        {
                            if (!ItemFound)
                            {
                                Itm = Itm2;
                                ItemFound = true;
                            }

                            Qcount++;
                        }
                    }
                    label5.Text = "Q:" + Qcount;

                    if (Itm.QuId != null)
                    {
                        WebRequest ReqStart = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Server"].Trim() + "/mc.svc/files/upload/" + Itm.QuId + "/start");
                        //ReqStart.Timeout = Timeout.Infinite;
                        //ReqStart.KeepAlive = true;

                        try
                        {
                            ReqStart.GetResponse();

                        }
                        catch
                        {

                        }


                        string[] SrcDir = Itm.Filename.Split('\\');

                        string Source = Itm.ConvertDirectory + SrcDir[0] + "\\" + Path.GetFileNameWithoutExtension(Itm.SrcDirectory + Itm.Filename) + Itm.FilenameSuffix;
                        string ReplaceSourceServer = System.Configuration.ConfigurationSettings.AppSettings["ReplaceSourceServer"].Trim();
                        if (ReplaceSourceServer.Trim().Length > 0)
                        {
                            Source = Source.Replace("E:", ReplaceSourceServer);
                        }

                       

                        string Origin = "";
                        string DestDir = "";
                        if (Itm.DestDirectory.ToString().Trim().Length > 0)
                        {
                            DestDir = DateTime.Parse(Itm.DateTime_Insert).ToString(Itm.DestDirectory) + "/";
                        }
                        else
                        {
                            //MessageBox.Show("Org:"+Itm.Origin);
                            string[] tmpOrg = Itm.Origin.Split(new string[] { "\\" },100,StringSplitOptions.RemoveEmptyEntries);
                            //MessageBox.Show("tmpOrg len:" + tmpOrg.Length);

                            //Iktv:for press for index start from 1 but changed to zero for iktv and added check lenght of item i >1
                            for (int i = 0; i < tmpOrg.Length - 1; i++)
                            {
                                //MessageBox.Show(tmpOrg[i]);
                                if(tmpOrg[i].Trim().Length>1)
                                Origin += tmpOrg[i] + "\\";
                            }
                            DestDir = Origin.Replace(" ", "-").Replace("(", "-").Replace(")", "-").Replace("&", "-");
                            //MessageBox.Show("DestDir:" + DestDir);

                        }
                        string Dest = "";
                        if(Itm.FilenameSuffix.ToLower().Contains("sprite"))
                         Dest = Itm.ServerIp + DestDir + Path.GetFileNameWithoutExtension(Itm.SrcDirectory +"\\"+ Itm.Filename) + Itm.FilenameSuffix.Replace("\\", "");
                        else
                        Dest = Itm.ServerIp + DestDir + Path.GetFileNameWithoutExtension(Itm.SrcDirectory + Itm.Filename) + Itm.FilenameSuffix;
                       // MessageBox.Show("Dest:"+Dest);
                      //  MessageBox.Show(Dest);





                        if (Itm.ServerIp.ToLower().Contains("ftp"))
                        {
                            //Added to replace % from  sprit filename:
                            Source = Source.Replace("%03d", "").Replace("\\_", "_");


                            Dest = Dest.Replace("%03d", "");//.Replace("\\", "");

                            LogWriter("S:" + Source);
                            LogWriter("D:" + Dest);

                            dataGridView1.Rows.Add(Itm.QuId, Source, Dest, "", Itm.ServerIp, Itm.ServerUser, Itm.ServerPass, Itm.DestDirectory, Itm.Retry);

                            if (File.Exists(Source))
                            {
                                if (!Dest.ToLower().Contains("_youtube.mp4"))
                                {
                                    try
                                    {
                                        FtpWebRequest request2 = (FtpWebRequest)FtpWebRequest.Create(Itm.ServerIp + DestDir);
                                        request2.Method = WebRequestMethods.Ftp.MakeDirectory;
                                        request2.Credentials = new NetworkCredential(Itm.ServerUser, Itm.ServerPass);

                                        if (System.Configuration.ConfigurationSettings.AppSettings["FtpActive"].Trim() == "0")
                                        {
                                            request2.UsePassive = true;

                                        }
                                        else
                                        {
                                            request2.UsePassive = false;

                                        }

                                        request2.UseBinary = true;
                                        request2.KeepAlive = false;
                                        using (var resp = (FtpWebResponse)request2.GetResponse())
                                        {
                                            if (resp.StatusCode == FtpStatusCode.PathnameCreated)
                                            {
                                                Console.WriteLine(resp.StatusCode);
                                                resp.Close();
                                            }
                                        }
                                    }
                                    catch
                                    {

                                    }


                                    try
                                    {
                                        var ftpWebRequest = (FtpWebRequest)WebRequest.Create(Dest);
                                        ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                                        ftpWebRequest.Credentials = new NetworkCredential(Itm.ServerUser, Itm.ServerPass);

                                        if (System.Configuration.ConfigurationSettings.AppSettings["FtpActive"].Trim() == "0")
                                        {
                                            ftpWebRequest.UsePassive = true;

                                        }
                                        else
                                        {
                                            ftpWebRequest.UsePassive = false;

                                        }
                                        ftpWebRequest.UseBinary = true;
                                        ftpWebRequest.KeepAlive = false;
                                        using (var inputStream = File.OpenRead(Source))
                                        using (var outputStream = ftpWebRequest.GetRequestStream())
                                        {
                                            var buffer = new byte[32 * 1024];
                                            int readBytesCount;
                                            long length = inputStream.Length;
                                            long r = 0;
                                            progressBar1.Maximum = (int)length;
                                            while ((readBytesCount = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                                            {
                                                outputStream.Write(buffer, 0, readBytesCount);
                                                r += readBytesCount;
                                                progressBar1.Value = (int)r;
                                                Application.DoEvents();
                                            }
                                            outputStream.Close();
                                        }
                                        WebRequest ReqDone = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Server"].Trim() + "/mc.svc/files/upload/" + Itm.QuId + "/done");
                                        //ReqDone.Timeout = Timeout.Infinite;
                                        //ReqDone.KeepAlive = true;

                                        
                                        //Check File is Exist on server:
                                        var requestExist = (FtpWebRequest)WebRequest.Create(Dest);
                                        requestExist.Credentials = new NetworkCredential(Itm.ServerUser, Itm.ServerPass);
                                        requestExist.Method = WebRequestMethods.Ftp.GetFileSize;

                                        try
                                        {
                                            FtpWebResponse responseExist = (FtpWebResponse)requestExist.GetResponse();
                                            ReqDone.GetResponse();
                                            if (!File.Exists(Source))
                                            {
                                                WebRequest reqUndoneConvert = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Server"].Trim() + "/mc.svc/files/convert/" + Itm.QcId + "/undone");
                                                reqUndoneConvert.GetResponse();
                                            }
                                        }
                                        catch (WebException ex)
                                        {                                       
                                            LogWriter("Er Upload FTP:" + ex.Message);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        WebRequest ReqDone = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Server"].Trim() + "/mc.svc/files/upload/" + Itm.QuId + "/done");
                                        //ReqDone.Timeout = Timeout.Infinite;
                                        //ReqDone.KeepAlive = true;
                                        ReqDone.GetResponse();
                                    }
                                    catch { }
                                }
                            }
                            else
                            {
                                LogWriter("File Not exist:"+Source);
                                WebRequest reqUndoneConvert = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Server"].Trim() + "/mc.svc/files/convert/"+Itm.QcId+"/undone");
                                reqUndoneConvert.GetResponse();
                            }
                        }
                        try
                        {
                            if (Itm.ServerIp.ToLower().StartsWith("\\\\"))
                            {

                                //Added to replace % from  sprit filename:
                                Source = Source.Replace("%03d", "").Replace("\\_sprite", "_sprite");

                                Dest = Dest.Replace("%03d", "").Replace("\\_sprite", "_sprite");

                                LogWriter("S:" + Source);
                                LogWriter("D:" + Dest);


                                dataGridView1.Rows.Add(Itm.QuId, Source, Dest, "", Itm.ServerIp, Itm.ServerUser, Itm.ServerPass, Itm.DestDirectory, Itm.Retry);
                                if (File.Exists(Source))
                                {
                                    if (!Directory.Exists(Itm.ServerIp + DestDir))
                                    {
                                        Directory.CreateDirectory(Itm.ServerIp + DestDir);
                                    }
                                    File.Copy(Source, Dest, true);
                                    WebRequest ReqDone = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Server"].Trim() + "/mc.svc/files/upload/" + Itm.QuId + "/done");
                                    //ReqDone.Timeout = Timeout.Infinite;
                                    //ReqDone.KeepAlive = true;
                                    if (File.Exists(Dest))
                                    {
                                        ReqDone.GetResponse();
                                        if (!File.Exists(Source))
                                        {
                                            WebRequest reqUndoneConvert = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Server"].Trim() + "/mc.svc/files/convert/" + Itm.QcId + "/undone");
                                            reqUndoneConvert.GetResponse();
                                        }
                                    }
                                }
                                else
                                {
                                    LogWriter("File Not exist:" + Source);
                                    WebRequest reqUndoneConvert = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Server"].Trim() + "/mc.svc/files/convert/" + Itm.QcId + "/undone");
                                    reqUndoneConvert.GetResponse();
                                }
                            }
                        }
                        catch
                        {

                        }

                    }
                }
            }
            catch (Exception excp)
            {
                LogWriter(excp.Message);
            }
        }
        protected void LogWriter(string LogText)
        {
            if (richTextBox1.Lines.Length > 200)
            {
                richTextBox1.Text = "";
            }
            richTextBox1.Text += (LogText) + " [ " + DateTime.Now.ToString("hh:mm:ss") + " ] \n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
            Application.DoEvents();
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            ProccessList();
            timer1.Enabled = true;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            btnStart_Click(null, null);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (this.Height == 271)
            {
                this.Height = 180;
            }
            else
            {
                this.Height = 271;
            }
        }
    }
    public class UploadQueue
    {
        public string ServerIp { get; set; }
        public string ServerUser { get; set; }
        public string ServerPass { get; set; }
        public string DestDirectory { get; set; }
        public string Priority { get; set; }
        public string SrcDirectory { get; set; }
        public string QuId { get; set; }
        public string ConvertDirectory { get; set; }
        public string Filename { get; set; }
        public string FilenameSuffix { get; set; }
        public string Retry { get; set; }
        public string DateTime_Insert { get; set; }
        public string QcId { get; set; }
        public string Origin { get; set; }

    }
}
