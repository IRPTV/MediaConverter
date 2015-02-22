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
        void Temp_EV_copyComplete()
        {
            this.Invoke(new MethodInvoker(delegate()
            {
                openFileDialog1.FileName="";
                label2.Text = "Please select file";

                if (File.Exists(_DestFile))
                {
                    //Call Service:
                    WebRequest request = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["Service"].Trim());

                    request.Method = "POST";

                    string postData = "FileName=" + _DateDir + "\\" + _FileName.Replace(" ", "-") + "&UserId=" + System.Configuration.ConfigurationSettings.AppSettings["UserId"].Trim();

                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                    request.ContentType = "application/x-www-form-urlencoded";

                    request.ContentLength = byteArray.Length;

                    Stream dataStream = request.GetRequestStream();

                    dataStream.Write(byteArray, 0, byteArray.Length);

                    dataStream.Close();

                    WebResponse response = request.GetResponse();
                    MessageBox.Show("File added to convert queue", "Upload", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }));

        }
        void Temp_EV_copyCanceled(List<CopyFiles.CopyFiles.ST_CopyFileDetails> filescopied)
        {
            //throw new NotImplementedException();
            MessageBox.Show("عملیات کپی متوقف شد");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "mpg files (*.mpg)|*.mpg|mp4 files (*.mp4)|*.mp4|Avi files (*.avi)|*.avi";
            openFileDialog1.Title = "Select Avi Or Mp4 Or Mpg Files";
            openFileDialog1.Multiselect = true;
            openFileDialog1.ShowDialog();
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            label2.Text = openFileDialog1.FileName.Replace(" ", "-");
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.FileName.Length>0)
            {
                richTextBox2.Text += "\n===================\n";
                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                richTextBox2.ScrollToCaret();
                Application.DoEvents();

                _SourceFile = openFileDialog1.FileName;
                _FileName = Path.GetFileName(openFileDialog1.FileName).Replace(" ","-");
                _Path = System.Configuration.ConfigurationSettings.AppSettings["DestPath"].Trim() + "\\" + DateTime.Now.ToString("yyyyMMdd");
                _DestFile = _Path + "\\" + Path.GetFileName(openFileDialog1.FileName).Replace(" ", "-");
                _DateDir = DateTime.Now.ToString("yyyyMMdd");
                
                
                DirectoryInfo Dir = new DirectoryInfo(_Path);
                if (!Dir.Exists)
                {
                    Dir.Create();
                    richTextBox2.Text += "Temp Directory Created: " + Dir.ToString() + "\n";
                    richTextBox2.SelectionStart = richTextBox2.Text.Length;
                    richTextBox2.ScrollToCaret();
                    Application.DoEvents();
                }
                richTextBox2.Text += "Start Copy To Local: " + _SourceFile.ToString() + "\n";
                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                richTextBox2.ScrollToCaret();
                Application.DoEvents();

                List<String> TempFiles = new List<String>();
                TempFiles.Add(_SourceFile);

                CopyFiles.CopyFiles Temp = new CopyFiles.CopyFiles(TempFiles, _DestFile);
                Temp.EV_copyCanceled += Temp_EV_copyCanceled;
                Temp.EV_copyComplete += Temp_EV_copyComplete;

                CopyFiles.DIA_CopyFiles TempDiag = new CopyFiles.DIA_CopyFiles();
                TempDiag.SynchronizationObject = this;
                Temp.CopyAsync(TempDiag);
            }
        }
    }
}
