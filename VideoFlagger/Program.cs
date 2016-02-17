using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Timers;
using System.IO;
using System.Configuration;

namespace VideoFlagger
{
    class FlagQueue
    {
        public string Command { get; set; }
        public string ConvertDirectory { get; set; }
        public string FileId { get; set; }
        public string Filename { get; set; }
        public string FilenameSuffix { get; set; }
        public string Kind { get; set; }//for thumbnail or video or ...
        public string QfId { get; set; }
        public string SrcDirectory { get; set; }
        public List<UploadQueue> UploadQueue { get; set; }
    }

    class UploadQueue
    {
        public string DestDirectory { get; set; }
        public string ServerIp { get; set; }
    }

    class Program
    {
        static bool _isFree;
        static string backServer;
        static string vttServer;
        static string vttReplace;
        static string serverCode;

        static void Main(string[] args)
        {
            backServer = (string)new AppSettingsReader().GetValue("backServer", typeof(String));
            vttServer = (string)new AppSettingsReader().GetValue("vttServer", typeof(String));
            vttReplace = (string)new AppSettingsReader().GetValue("vttReplace", typeof(String));
            serverCode = (string)new AppSettingsReader().GetValue("serverCode", typeof(String));

            var t = new Timer(12 * 1000);
            t.Elapsed += Elapsed;
            t.Enabled = true;

            go();

            while (Console.ReadLine() != "q") { }
        }

        static void Elapsed(object sender, EventArgs e)
        {
            if (_isFree)
            {
                go();
            }
        }

        static void go()
        {
            _isFree = false;
            using (var wc = new WebClient())
            {
                try
                {
                    var innerData = Encoding.UTF8.GetString(wc.DownloadData(backServer + "1/false/" + serverCode));
                    var ff = JsonConvert.DeserializeObject<IEnumerable<FlagQueue>>(innerData);

                    foreach (FlagQueue f in ff)
                    {
                        string file = f.ConvertDirectory + f.Filename.Remove(f.Filename.LastIndexOf(".")) + f.FilenameSuffix;
                        Console.WriteLine(DateTime.Now + "\tRunning Flagger...");
                        if (f.Kind == "0")
                        {
                            if (file.Contains("\\_sprite%03d.png"))
                            {
                                try
                                {
                                    File.Delete(file);
                                }
                                catch { }
                                file = file.Replace("\\_sprite%03d.png", string.Empty);
                                montage(file);
                            }
                            runM4Box(file, f.QfId);
                            foreach (UploadQueue uq in f.UploadQueue)
                            {
                                if (uq.ServerIp.StartsWith("\\"))
                                {
                                    var hh = vttServer + uq.ServerIp.Replace(vttReplace, string.Empty) + f.Filename;
                                    try
                                    {
                                        wc.DownloadData(hh);
                                    }
                                    catch { }
                                }
                            }
                        }
                        else
                        {
                            var innerData2 = Encoding.UTF8.GetString(wc.DownloadData(backServer + f.QfId + "/start"));
                            var innerData3 = Encoding.UTF8.GetString(wc.DownloadData(backServer + f.QfId + "/done"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now + "\t------ERROR-------" + ex.Message);
                }
            }
            Console.WriteLine(DateTime.Now + "\t--------------------Pass--------------");
            _isFree = true;
        }

        static void montage(string fileAddr)
        {
            var command = "\"" + fileAddr + "/*.png\" -tile 4x40 -geometry 100x80+0+0 \"" + fileAddr + "_sprite.png\"";
            ProcessStartInfo info = new ProcessStartInfo("C:\\Program Files\\GPAC\\montage.exe", command);
            Process p = new Process();
            p.StartInfo = info;
            p.EnableRaisingEvents = true;
            p.Start();

            string aa = "WEBVTT\n\n";
            DateTime dt = DateTime.Parse("1/1/1");

            for (int i = 0; i < 160; i++)
            {
                aa += dt.ToString("HH:mm:ss.000") + " --> " + dt.AddSeconds(45).ToString("HH:mm:ss.000") + "\n";
                aa += fileAddr + "_sprite.jpg#xywh=" + ((i % 4) * 100) + "," + ((i / 4) * 80) + ",100,80\n\n";
                dt = dt.AddSeconds(45);
            }
        }

        static void runM4Box(string fileAddr, string QfId)
        {
            var command = "-inter 0.5 \"" + fileAddr + "\"";
            //File.AppendAllText("e:\\og.txt", command + "\r\n");
            ProcessStartInfo info = new ProcessStartInfo("C:\\Program Files\\GPAC\\mp4box.exe", command);
            Process p = new Process();
            p.StartInfo = info;
            p.EnableRaisingEvents = true;
            p.Exited += (sender, e) =>
            {
                using (var wc = new WebClient())
                {
                    var innerData = Encoding.UTF8.GetString(wc.DownloadData(backServer + QfId + "/done"));
                }
            };
            using (var wc = new WebClient())
            {
                var innerData = Encoding.UTF8.GetString(wc.DownloadData(backServer + QfId + "/start"));
            }
            p.Start();
        }
    }
}
