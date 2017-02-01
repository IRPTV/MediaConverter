using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Feeder
{
    public partial class Form1 : Form
    {
        string _DestFile = "";
        string _SourceFile = "";
        string _FileName = "";
        string _DateDir = "";
        string _Path = "";
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["SourceService"].Trim());
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                var result = reader.ReadToEnd();
                stream.Dispose();
                reader.Dispose();
                List<PtnaItems> RvLst = JsonConvert.DeserializeObject<List<PtnaItems>>(result);
                List<PtnaItems> SortedList = RvLst.OrderBy(o => o.ModifiedDate).ToList();
                foreach (var item in SortedList)
                {
               // MessageBox.Show(item.FileName);
                if (item.ModifiedDate> DateTime.Parse(File.ReadAllLines(Path.GetDirectoryName(Application.ExecutablePath) + "\\LastJob.txt")[0]))
                    {
                        File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + "\\LastJob.txt", item.ModifiedDate.ToString());
                    //MessageBox.Show("MOD:"+item.ModifiedDate+"**"+item.FileName);
                    richTextBox2.Text = "\n===================\n";
                        richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        richTextBox2.ScrollToCaret();
                        Application.DoEvents();

                        //_SourceFile = "\\\\192.168.20.155\\Final" + item.FileName;
                        _SourceFile = item.FileName;

                        _FileName = Path.GetFileName(_SourceFile).Replace(" ", "-").Replace("(", "-").Replace(")", "-").Replace("&", "-");
                        _Path = System.Configuration.ConfigurationSettings.AppSettings["DestPath"].Trim() + "\\" + DateTime.Now.ToString("yyyyMMdd");
                        _DestFile = _Path + "\\" + Path.GetFileName(_SourceFile).Replace(" ", "-").Replace("(", "-").Replace(")", "-").Replace("&", "-");
                        _DateDir = DateTime.Now.ToString("yyyyMMdd");

                        label2.Text = _SourceFile;
                        label1.Text = DateTime.Now.ToString();

                        DirectoryInfo Dir = new DirectoryInfo(_Path);
                        if (!Dir.Exists)
                        {
                            Dir.Create();
                            richTextBox2.Text += "Temp Directory Created: " + Dir.ToString() + "\n";
                            richTextBox2.SelectionStart = richTextBox2.Text.Length;
                            richTextBox2.ScrollToCaret();
                            Application.DoEvents();
                        }
                        //richTextBox2.Text += "Start Copy To Local: " + _SourceFile.ToString() + "\n";
                        //richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //richTextBox2.ScrollToCaret();
                        //Application.DoEvents();
                        //  if (File.Exists(_SourceFile.Replace("source", "source\\logo")))
                        //    break;


                        //CopyFiles.CopyFiles Temp = new CopyFiles.CopyFiles(_SourceFile, _DestFile);
                        //Temp.EV_copyCanceled += Temp_EV_copyCanceled;
                        //Temp.EV_copyComplete += Temp_EV_copyComplete;

                        //CopyFiles.DIA_CopyFiles TempDiag = new CopyFiles.DIA_CopyFiles();
                        //TempDiag.SynchronizationObject = this;
                        //Temp.Copy();

                        ////Copy File to Duplicate Dest Path:
                        try
                        {
                            var DuplicateDestPath = System.Configuration.ConfigurationSettings.AppSettings["DuplicateDestPath"].Trim();
                            if (DuplicateDestPath.Length > 0)
                            {
                                string DupDestDir = "";
                                string[] tmpOrg = _SourceFile.Split(new string[] { "\\" }, 100, StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 1; i < tmpOrg.Length - 1; i++)
                                {
                                    DupDestDir += tmpOrg[i] + "\\";
                                }
                                DupDestDir = DupDestDir.Replace(" ", "-").Replace("(", "-").Replace(")", "-").Replace("&", "-");
                                Directory.CreateDirectory(DuplicateDestPath + "\\" + DupDestDir);
                                File.Copy(_SourceFile, DuplicateDestPath + "\\" + DupDestDir + "\\" + _FileName, true);

                            }
                        }
                        catch { }






                        File.Copy(_SourceFile, _DestFile,true);
                        if (File.Exists(_DestFile))
                        {
                        //MessageBox.Show("EXIST:"+_DestFile);
                        WebRequest request2 = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim());

                            request2.Method = "POST";

                            string postData = "FileName=" + _DateDir + "\\" + _FileName.Replace(" ", "-") + "&UserId=" + System.Configuration.ConfigurationSettings.AppSettings["UserId"].Trim() + "&Origin=" + _SourceFile;

                            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                            request2.ContentType = "application/x-www-form-urlencoded";

                            request2.ContentLength = byteArray.Length;

                            Stream dataStream = request2.GetRequestStream();

                            dataStream.Write(byteArray, 0, byteArray.Length);

                            dataStream.Close();

                            WebResponse response3 = request2.GetResponse();
                            timer1.Enabled = true;
                        }
                    }
                    else
                    {
                        timer1.Enabled = true;
                    }
                }
            }
            catch
            {
                timer1.Enabled = true;
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            timer1_Tick(null, null);
        }
    }
}
