using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Feeder
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
        protected void CopyLocal(string SourceFile, string DestFile, string TemDir)
        {
            richTextBox2.Text += "\n===================\n";
            richTextBox2.SelectionStart = richTextBox2.Text.Length;
            richTextBox2.ScrollToCaret();
            Application.DoEvents();


            DirectoryInfo Dir = new DirectoryInfo(TemDir);
            if (!Dir.Exists)
            {
                Dir.Create();
                richTextBox2.Text += "Temp Directory Created: " + Dir.ToString() + "\n";
                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                richTextBox2.ScrollToCaret();
                Application.DoEvents();
            }
            richTextBox2.Text += "Start Copy To Local: " + SourceFile.ToString() + "\n";
            richTextBox2.SelectionStart = richTextBox2.Text.Length;
            richTextBox2.ScrollToCaret();
            Application.DoEvents();

            List<String> TempFiles = new List<String>();
            TempFiles.Add(SourceFile);

            CopyFiles.CopyFiles Temp = new CopyFiles.CopyFiles(TempFiles, DestFile);
            Temp.EV_copyCanceled += Temp_EV_copyCanceled;
            Temp.EV_copyComplete += Temp_EV_copyComplete;

            CopyFiles.DIA_CopyFiles TempDiag = new CopyFiles.DIA_CopyFiles();
            TempDiag.SynchronizationObject = this;
            Temp.CopyAsync(TempDiag);

        }
        void Temp_EV_copyComplete()
        {
            this.Invoke(new MethodInvoker(delegate()
            {
               

            }));

        }
        void Temp_EV_copyCanceled(List<CopyFiles.CopyFiles.ST_CopyFileDetails> filescopied)
        {
            //throw new NotImplementedException();
            MessageBox.Show("عملیات کپی متوقف شد");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "mp4 files (*.mp4)|*.mp4|Avi files (*.avi)|*.avi";
            openFileDialog1.Title = "Select Avi Or Mp4 Files";
            openFileDialog1.Multiselect = true;
            openFileDialog1.ShowDialog();
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
