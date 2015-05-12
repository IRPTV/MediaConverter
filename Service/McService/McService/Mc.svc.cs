using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using McService.MyDBTableAdapters;
using Newtonsoft.Json;
using System.IO;
using System.Web;
using System.Collections.Specialized;
using System.Web.Configuration;

namespace McService
{
    [MessageContract(IsWrapped = false)]
    public class Mc : IMc
    {
        #region User

        public List<UserDetails> UsersList()
        {
            UsersTableAdapter Users_Ta = new UsersTableAdapter();
            MyDB.UsersDataTable Users_Dt = Users_Ta.Users_SelectAll();
            List<UserDetails> Lst = new List<UserDetails>();
            for (int i = 0; i < Users_Dt.Rows.Count; i++)
            {
                UserDetails U = new UserDetails
               {
                   Name = Users_Dt[i]["name"].ToString(),
                   PassWord = Users_Dt[i]["PassWord"].ToString(),
                   UserName = Users_Dt[i]["UserName"].ToString(),
                   CategoryId = Users_Dt[i]["Catid"].ToString(),
                   UserId = Users_Dt[i]["UID"].ToString()
               };
                Lst.Add(U);
            }
            return Lst;
        }

        public string UserDetail(string Username)
        {
            UsersTableAdapter Users_Ta = new UsersTableAdapter();
            MyDB.UsersDataTable Users_Dt = Users_Ta.Select_User_ByName(Username);
            return JsonConvert.SerializeObject(Users_Dt);
        }

        public UserDetails LoginUser(Stream Data)
        {
            string body = new StreamReader(Data).ReadToEnd();
            NameValueCollection nvc = HttpUtility.ParseQueryString(body);


            UsersTableAdapter Users_Ta = new UsersTableAdapter();
            MyDB.UsersDataTable Users_Dt = Users_Ta.User_SelectByUserPass(nvc[0].ToString(), nvc[1].ToString());

            if (Users_Dt.Rows.Count == 1)
            {
                UserDetails U = new UserDetails
                {
                    Name = Users_Dt[0]["name"].ToString(),
                    PassWord = Users_Dt[0]["PassWord"].ToString(),
                    UserName = Users_Dt[0]["UserName"].ToString(),
                    CategoryId = Users_Dt[0]["Catid"].ToString(),
                    UserId = Users_Dt[0]["UID"].ToString()
                };
                return U;
            }
            else
            {
                UserDetails U = new UserDetails();
                return U;
            }


        }
        #endregion

        #region Files
        public List<UserFiles> UserFiles(string UserId)
        {
            ServiceTableAdapter Ta = new ServiceTableAdapter();
            MyDB.DataTable1DataTable Dt = Ta.User_Files_SelectTop(int.Parse(UserId));

            List<UserFiles> Lst = new List<UserFiles>();

            for (int i = 0; i < Dt.Rows.Count; i++)
            {
                UserFiles ff = new UserFiles();
                ff.Filename = Dt[i]["filename"].ToString();
                ff.Duration = Dt[i]["Duration"].ToString();
                ff.FileSize = Dt[i]["FileSize"].ToString();
                ff.UserID = Dt[i]["UserID"].ToString();
                ff.Error = Dt[i]["Error"].ToString();
                ff.ErrorLog = Dt[i]["ErrorLog"].ToString();


                //Convert:
                MyDB.DataTable1DataTable Dt_Convert = Ta.User_Files_Convert(int.Parse(Dt[i]["fid"].ToString()));
                List<UserFilesConvert> cnvlist = new List<UserFilesConvert>();
                for (int j = 0; j < Dt_Convert.Rows.Count; j++)
                {
                    UserFilesConvert cnv = new UserFilesConvert();
                    cnv.Convert_Datetime_Done = Dt_Convert[j]["Datetime_Done"].ToString();
                    cnv.Convert_Datetime_Insert = Dt_Convert[j]["Datetime_Insert"].ToString();
                    cnv.Convert_Datetime_Start = Dt_Convert[j]["Datetime_Start"].ToString();
                    cnv.Converted = Dt_Convert[j]["Converted"].ToString();
                    cnv.ProfileTitle = Dt_Convert[j]["Title"].ToString();
                    cnvlist.Add(cnv);
                }
                ff.FilesCnvrt = cnvlist;

                //Flag:
                MyDB.DataTable1DataTable Dt_Flag = Ta.User_Files_Select_Flag(int.Parse(Dt[i]["fid"].ToString()));
                List<UserFilesFlag> flglist = new List<UserFilesFlag>();
                for (int p = 0; p < Dt_Flag.Rows.Count; p++)
                {
                    UserFilesFlag flg = new UserFilesFlag();
                    flg.Flag_Datetime_Done = Dt_Flag[p]["Datetime_Done"].ToString();
                    flg.Flag_Datetime_Insert = Dt_Flag[p]["Datetime_Insert"].ToString();
                    flg.Flag_Datetime_Start = Dt_Flag[p]["Datetime_Start"].ToString();
                    flg.Flaged = Dt_Flag[p]["isdone"].ToString();
                    flg.ProfileTitle = Dt_Flag[p]["Title"].ToString();
                    flglist.Add(flg);
                }
                ff.FilesFlg = flglist;


                //Upload:
                MyDB.DataTable1DataTable Dt_Upload = Ta.User_Files_SelectUpload(int.Parse(Dt[i]["fid"].ToString()));
                List<UserFilesUpload> upllist = new List<UserFilesUpload>();
                for (int m = 0; m < Dt_Upload.Rows.Count; m++)
                {
                    UserFilesUpload upl = new UserFilesUpload();
                    upl.Upload_Datetime_Done = Dt_Upload[m]["Datetime_Done"].ToString();
                    upl.Upload_Datetime_Insert = Dt_Upload[m]["Datetime_Insert"].ToString();
                    upl.Upload_Datetime_Start = Dt_Upload[m]["Datetime_Start"].ToString();
                    upl.Uploaded = Dt_Upload[m]["Uploaded"].ToString();
                    upl.ServerIp = Dt_Upload[m]["ServerIp"].ToString();
                    upl.ProfileTitle = Dt_Upload[m]["Title"].ToString();
                    upllist.Add(upl);
                }
                ff.FilesUpld = upllist;









                Lst.Add(ff);
            }
            return Lst;
        }

        public List<ConvertQueue> GetConvertQueue(string TopCount, string Converted, string ProfileID,string ServerCode)
        {
              List<ConvertQueue> Lst = new List<ConvertQueue>();
              try
              {
                  ServiceTableAdapter Cnvrt_Ta = new ServiceTableAdapter();
                  MyDB.DataTable1DataTable Cnvrt_Dt = Cnvrt_Ta.Select_ConvertQueue(int.Parse(TopCount), bool.Parse(Converted), int.Parse(ProfileID), short.Parse(ServerCode));

                  for (int i = 0; i < Cnvrt_Dt.Rows.Count; i++)
                  {

                      ConvertQueue Cnvrt = new ConvertQueue();
                      Cnvrt.Command = Cnvrt_Dt[i]["Command"].ToString();
                      Cnvrt.ConvertDirectory = Cnvrt_Dt[i]["ConvertDirectory"].ToString();
                      Cnvrt.ConvertId = Cnvrt_Dt[i]["ConvertId"].ToString();
                      Cnvrt.FileId = Cnvrt_Dt[i]["FileId"].ToString();
                      Cnvrt.Filename = Cnvrt_Dt[i]["Filename"].ToString();
                      Cnvrt.FilenameSuffix = Cnvrt_Dt[i]["FilenameSuffix"].ToString();
                      Cnvrt.ProfileKind = Cnvrt_Dt[i]["ProfileKind"].ToString();
                      Cnvrt.SrcDirectory = Cnvrt_Dt[i]["SrcDirectory"].ToString();
                      Lst.Add(Cnvrt);
                  }

              }
              catch
              {
              }
            return Lst;
        }
        public List<ConvertQueue> GetConvertQueueAll(string TopCount, string Converted)
        {           
            List<ConvertQueue> Lst = new List<ConvertQueue>();

            try
            {
                ServiceTableAdapter Cnvrt_Ta = new ServiceTableAdapter();
                MyDB.DataTable1DataTable Cnvrt_Dt = Cnvrt_Ta.Select_ConvertQAll(int.Parse(TopCount), bool.Parse(Converted));
                for (int i = 0; i < Cnvrt_Dt.Rows.Count; i++)
                {

                    ConvertQueue Cnvrt = new ConvertQueue();
                    Cnvrt.Command = Cnvrt_Dt[i]["Command"].ToString();
                    Cnvrt.ConvertDirectory = Cnvrt_Dt[i]["ConvertDirectory"].ToString();
                    Cnvrt.ConvertId = Cnvrt_Dt[i]["ConvertId"].ToString();
                    Cnvrt.FileId = Cnvrt_Dt[i]["FileId"].ToString();
                    Cnvrt.Filename = Cnvrt_Dt[i]["Filename"].ToString();
                    Cnvrt.FilenameSuffix = Cnvrt_Dt[i]["FilenameSuffix"].ToString();
                    Cnvrt.ProfileKind = Cnvrt_Dt[i]["ProfileKind"].ToString();
                    Cnvrt.SrcDirectory = Cnvrt_Dt[i]["SrcDirectory"].ToString();
                    Lst.Add(Cnvrt);
                }

            }
            catch { }
            return Lst;
        }

        public int SetConvertQueueStart(string ConvertId)
        {
            ServiceTableAdapter Cnvrt_Ta = new ServiceTableAdapter();
            return Cnvrt_Ta.Convert_Queue_Start(long.Parse(ConvertId));
        }
        public void SetConvertQueuedelete(string ConvertId)
        {
            ServiceTableAdapter Cnvrt_Ta = new ServiceTableAdapter();
            Cnvrt_Ta.Delete_ConvertQ(long.Parse(ConvertId));
        }

        public int SetConvertQueueDone(string ConvertId)
        {
            ServiceTableAdapter Cnvrt_Ta = new ServiceTableAdapter();
            Cnvrt_Ta.Convert_Queue_Done(long.Parse(ConvertId));
            return Cnvrt_Ta.Flag_Queue_Insert(long.Parse(ConvertId));
        }

        public List<FlagQueue> GetFlagQueue(string TopCount, string Flaged, string ServerCode)
        {
            ServiceTableAdapter Flag_Ta = new ServiceTableAdapter();
            MyDB.DataTable1DataTable Flag_Dt = Flag_Ta.Select_FlagQueue(int.Parse(TopCount), bool.Parse(Flaged),short.Parse(ServerCode));
            List<FlagQueue> Lst = new List<FlagQueue>();

            for (int i = 0; i < Flag_Dt.Rows.Count; i++)
            {

                FlagQueue Flag = new FlagQueue();
                Flag.Command = Flag_Dt[i]["Command"].ToString();
                Flag.ConvertDirectory = Flag_Dt[i]["ConvertDirectory"].ToString();
                Flag.QfId = Flag_Dt[i]["QfId"].ToString();
                Flag.FileId = Flag_Dt[i]["FileId"].ToString();
                Flag.Filename = Flag_Dt[i]["Filename"].ToString();
                Flag.FilenameSuffix = Flag_Dt[i]["FilenameSuffix"].ToString();
                Flag.Kind = Flag_Dt[i]["Kind"].ToString();
                Flag.SrcDirectory = Flag_Dt[i]["SrcDirectory"].ToString();

                //Get Upload Q:
                MyDB.DataTable1DataTable FlagUpload_Dt = Flag_Ta.Select_UploadQfor_Flag(long.Parse(Flag.QfId));
                List<UploadQueue> UpQList = new List<UploadQueue>();
                for (int j = 0; j < FlagUpload_Dt.Rows.Count; j++)
                {
                    UploadQueue uq = new UploadQueue();
                    uq.DestDirectory = FlagUpload_Dt[j]["DestDirectory"].ToString();
                    uq.ServerIp = FlagUpload_Dt[j]["ServerIp"].ToString();
                    uq.ServerPass = FlagUpload_Dt[j]["ServerPass"].ToString();
                    uq.ServerUser = FlagUpload_Dt[j]["ServerUser"].ToString();
                    UpQList.Add(uq);
                }
                Flag.UploadQueue = UpQList;
                Lst.Add(Flag);
            }

            return Lst;
        }
        public List<FlagQueue> GetFlagQueueDefaultServer(string TopCount, string Flaged)
        {
            ServiceTableAdapter Flag_Ta = new ServiceTableAdapter();
            MyDB.DataTable1DataTable Flag_Dt = Flag_Ta.Select_FlagQueue(int.Parse(TopCount), bool.Parse(Flaged),1);
            List<FlagQueue> Lst = new List<FlagQueue>();

            for (int i = 0; i < Flag_Dt.Rows.Count; i++)
            {
                FlagQueue Flag = new FlagQueue();
                Flag.Command = Flag_Dt[i]["Command"].ToString();
                Flag.ConvertDirectory = Flag_Dt[i]["ConvertDirectory"].ToString();
                Flag.QfId = Flag_Dt[i]["QfId"].ToString();
                Flag.FileId = Flag_Dt[i]["FileId"].ToString();
                Flag.Filename = Flag_Dt[i]["Filename"].ToString();
                Flag.FilenameSuffix = Flag_Dt[i]["FilenameSuffix"].ToString();
                Flag.Kind = Flag_Dt[i]["Kind"].ToString();
                Flag.SrcDirectory = Flag_Dt[i]["SrcDirectory"].ToString();

                //Get Upload Q:
                MyDB.DataTable1DataTable FlagUpload_Dt = Flag_Ta.Select_UploadQfor_Flag(long.Parse(Flag.QfId));
                List<UploadQueue> UpQList = new List<UploadQueue>();
                for (int j = 0; j < FlagUpload_Dt.Rows.Count; j++)
                {
                    UploadQueue uq = new UploadQueue();
                    uq.DestDirectory = FlagUpload_Dt[j]["DestDirectory"].ToString();
                    uq.ServerIp = FlagUpload_Dt[j]["ServerIp"].ToString();
                    uq.ServerPass = FlagUpload_Dt[j]["ServerPass"].ToString();
                    uq.ServerUser = FlagUpload_Dt[j]["ServerUser"].ToString();
                    UpQList.Add(uq);
                }
                Flag.UploadQueue = UpQList;
                Lst.Add(Flag);
            }

            return Lst;
        }
        public int SetFlagQueueStart(string FlagId)
        {
            ServiceTableAdapter Cnvrt_Ta = new ServiceTableAdapter();
            return Cnvrt_Ta.Update_Flag_Start(long.Parse(FlagId));
        }

        public int SetFlagQueueDone(string FlagId)
        {

            ServiceTableAdapter Flag_Ta = new ServiceTableAdapter();
            Flag_Ta.Update_Flag_Done(long.Parse(FlagId));
            MyDB.DataTable1DataTable Dt = Flag_Ta.Select_Flag_Q_ById(long.Parse(FlagId));
            for (int i = 0; i < Dt.Rows.Count; i++)
            {
                Flag_Ta.Insert_Upload_Queue(long.Parse(Dt[i]["QcId"].ToString()), int.Parse(Dt[i]["PathId"].ToString()));
            }
            return Dt.Rows.Count;

        }

        public List<UploadQueue> GetUploadQueue(string TopCount, string ServerCode)
        {
            ServiceTableAdapter Upload_Ta = new ServiceTableAdapter();
            MyDB.DataTable1DataTable Upload_Dt = Upload_Ta.Select_Upload_Q(int.Parse(TopCount),short.Parse(ServerCode));
            List<UploadQueue> Lst = new List<UploadQueue>();

            for (int i = 0; i < Upload_Dt.Rows.Count; i++)
            {
                UploadQueue Flag = new UploadQueue();
                Flag.ConvertDirectory = Upload_Dt[i]["ConvertDirectory"].ToString();
                Flag.DestDirectory = Upload_Dt[i]["DestDirectory"].ToString();
                Flag.Filename = Upload_Dt[i]["Filename"].ToString();
                Flag.Priority = Upload_Dt[i]["Priority"].ToString();
                Flag.QuId = Upload_Dt[i]["QuId"].ToString();
                Flag.ServerIp = Upload_Dt[i]["ServerIp"].ToString();
                Flag.ServerPass = Upload_Dt[i]["ServerPass"].ToString();
                Flag.ServerUser = Upload_Dt[i]["ServerUser"].ToString();
                Flag.SrcDirectory = Upload_Dt[i]["SrcDirectory"].ToString();
                Flag.FilenameSuffix = Upload_Dt[i]["FilenameSuffix"].ToString();
                Flag.Retry = Upload_Dt[i]["Retry"].ToString();
                Flag.DateTime_Insert = Upload_Dt[i]["DateTime_Insert"].ToString();
                Flag.QcId = Upload_Dt[i]["QcId"].ToString();

                if (int.Parse(Flag.Retry) > 1000)
                {
                    Upload_Ta.Update_Upload_Q_Done(long.Parse(Flag.QuId));
                }
                else
                {
                    Lst.Add(Flag);
                }
            }

            return Lst;
        }
        public List<UploadQueue> GetUploadQueueServer(string TopCount, string Ip, string ServerCode)
        {
            ServiceTableAdapter Upload_Ta = new ServiceTableAdapter();
            MyDB.DataTable1DataTable Upload_Dt = Upload_Ta.Select_UploadQ_ByServer(int.Parse(TopCount), Ip, short.Parse(ServerCode));
            List<UploadQueue> Lst = new List<UploadQueue>();

            for (int i = 0; i < Upload_Dt.Rows.Count; i++)
            {
                UploadQueue Flag = new UploadQueue();
                Flag.ConvertDirectory = Upload_Dt[i]["ConvertDirectory"].ToString();
                Flag.DestDirectory = Upload_Dt[i]["DestDirectory"].ToString();
                Flag.Filename = Upload_Dt[i]["Filename"].ToString();
                Flag.Priority = Upload_Dt[i]["Priority"].ToString();
                Flag.QuId = Upload_Dt[i]["QuId"].ToString();
                Flag.ServerIp = Upload_Dt[i]["ServerIp"].ToString();
                Flag.ServerPass = Upload_Dt[i]["ServerPass"].ToString();
                Flag.ServerUser = Upload_Dt[i]["ServerUser"].ToString();
                Flag.SrcDirectory = Upload_Dt[i]["SrcDirectory"].ToString();
                Flag.FilenameSuffix = Upload_Dt[i]["FilenameSuffix"].ToString();
                Flag.Retry = Upload_Dt[i]["Retry"].ToString();
                Flag.DateTime_Insert = Upload_Dt[i]["DateTime_Insert"].ToString();
                Flag.QcId = Upload_Dt[i]["QcId"].ToString();

                if (int.Parse(Flag.Retry) > 1000)
                {
                    Upload_Ta.Update_Upload_Q_Done(long.Parse(Flag.QuId));
                }
                else
                {
                    Lst.Add(Flag);
                }
            }

            return Lst;
        }

        public int SetUploadQueueStart(string UploadId)
        {
            ServiceTableAdapter Cnvrt_Ta = new ServiceTableAdapter();
            Cnvrt_Ta.Update_Retry_Upload(long.Parse(UploadId));
            return Cnvrt_Ta.Update_Upload_Q_Start(long.Parse(UploadId));
        }

        public int SetUploadQueueDone(string UploadId)
        {
            ServiceTableAdapter Cnvrt_Ta = new ServiceTableAdapter();
            return Cnvrt_Ta.Update_Upload_Q_Done(long.Parse(UploadId));
        }

        public void UploadFile(Stream fileContents)
        {


            byte[] buffer = new byte[10000];
            int bytesRead, totalBytesRead = 0;

            //string body = new StreamReader(fileContents).ReadToEnd();
            //NameValueCollection nvc = HttpUtility.ParseQueryString(body);

            //StreamReader reader = new StreamReader(fileContents);
            //FileStream file = File.Create(@"E:\FILES\SOURCE\" + nvc["filename"].ToString());

            do
            {
                bytesRead = fileContents.Read(buffer, 0, buffer.Length);
                totalBytesRead += bytesRead;
                // file.Write(buffer, 0, bytesRead);
            }
            while (bytesRead > 0);
            //  file.Close();
        }
        public void SetConvertQueueUnDone(string ConvertId)
        {
            ServiceTableAdapter Cnvrt_Ta = new ServiceTableAdapter();
            Cnvrt_Ta.Update_Convert_Tofalse(long.Parse(ConvertId));
            Cnvrt_Ta.Delete_UndoneUpload(long.Parse(ConvertId));
        }
        #endregion

        public List<LogoQueue> GetLogoQueue(string Count, string ServerCode)
        {
            List<LogoQueue> Lst = new List<LogoQueue>();

            try
            {
                ServiceTableAdapter Cnvrt_Ta = new ServiceTableAdapter();
                MyDB.DataTable1DataTable Cnvrt_Dt = Cnvrt_Ta.Select_Files_LogoQueue(int.Parse(Count), short.Parse(ServerCode));


                for (int i = 0; i < Cnvrt_Dt.Rows.Count; i++)
                {

                    LogoQueue Cnvrt = new LogoQueue();
                    Cnvrt.ConvertDirectory = Cnvrt_Dt[i]["ConvertDirectory"].ToString();
                    Cnvrt.FileId = Cnvrt_Dt[i]["FID"].ToString();
                    Cnvrt.Filename = Cnvrt_Dt[i]["Filename"].ToString();
                    Cnvrt.SrcDirectory = Cnvrt_Dt[i]["SrcDirectory"].ToString();
                    Cnvrt.Logo = Cnvrt_Dt[i]["Logo"].ToString();
                    Cnvrt.LogoFile = Cnvrt_Dt[i]["LogoFile"].ToString();
                    Lst.Add(Cnvrt);
                }
            }
            catch
            {

            }

            return Lst;
        }

        public int SetLogoDone(string LogoID)
        {
            ServiceTableAdapter Flag_Ta = new ServiceTableAdapter();
            Flag_Ta.Update_Files_LogoDone(long.Parse(LogoID));
            return 1;
        }
        public List<RepositoryFiles> SearchFiles(string Count, string query,string start,string End)
        {
            ServiceTableAdapter Ta = new ServiceTableAdapter();
            FilesTableAdapter Flag_Ta = new FilesTableAdapter();
            List<RepositoryFiles> Lst = new List<RepositoryFiles>();
            MyDB.FilesDataTable Cnvrt_Dt;

            DateTime startDt = DateTime.Now.AddYears(-10);

            DateTime endDt = DateTime.Now.AddYears(+10);


            //try
            //{
                if (start.Length > 5)
                {
                    startDt = DateTime.Parse(start);
                }

                if (End.Length > 5)
                {
                    endDt = DateTime.Parse(End).AddMinutes(1439);
                }
            //}
            //catch
            //{
            //   startDt = DateTime.Now.AddYears(-10);

            //   endDt = DateTime.Now.AddYears(+10);
            //}



            if (query.Trim().Length > 0)
            {
                Cnvrt_Dt = Flag_Ta.Select_Files_Like(int.Parse(Count), "%" + query + "%",startDt,endDt);

            }
            else
            {
                Cnvrt_Dt = Flag_Ta.Select_Files_Top(int.Parse(Count), startDt, endDt);

            }

            for (int i = 0; i < Cnvrt_Dt.Rows.Count; i++)
            {
                RepositoryFiles itm = new RepositoryFiles();
                itm.Filename = WebConfigurationManager.AppSettings["viewfilesaddress"] + "/" + Cnvrt_Dt[i]["Filename"].ToString().ToLower().Replace("\\", "/").Replace(".mpg", ".mp4");
                itm.Thumbnail = WebConfigurationManager.AppSettings["viewfilesaddress"] + "/" + Cnvrt_Dt[i]["Filename"].ToString().ToLower().Replace("\\", "/").Replace(".mp4", ".jpg").Replace(".mpg", ".jpg");
              
                itm.Id = Cnvrt_Dt[i]["FId"].ToString();
                itm.relativePath = Cnvrt_Dt[i]["Filename"].ToString();
                
                //Get Upload Q:
                MyDB.DataTable1DataTable FlagUpload_Dt = Ta.SelectUploadQbyFileID(long.Parse(itm.Id));
                if(FlagUpload_Dt.Rows.Count>0)
                {
                    itm.serverPath = FlagUpload_Dt[0]["ServerIp"].ToString();
                    itm.serverCode = FlagUpload_Dt[0]["ServerCode"].ToString();
                }               
                Lst.Add(itm);
            }

            return Lst;

        }

        public RepositoryFiles SearchFilesId(string id)
        {
            FilesTableAdapter Flag_Ta = new FilesTableAdapter();
            RepositoryFiles itm = new RepositoryFiles();
            MyDB.FilesDataTable Cnvrt_Dt = Flag_Ta.Select_Files_ByFid(long.Parse(id));

            if (Cnvrt_Dt.Rows.Count == 1)
            {

                itm.Filename = WebConfigurationManager.AppSettings["viewfilesaddress"] + "/SOURCE/" + Cnvrt_Dt[0]["Filename"].ToString().ToLower().Replace("\\", "/").Replace(".mpg", ".mp4");
                itm.Thumbnail = WebConfigurationManager.AppSettings["viewfilesaddress"] + "/" + Cnvrt_Dt[0]["Filename"].ToString().ToLower().Replace("\\", "/").Replace(".mp4", ".jpg").Replace(".mpg", ".jpg");
                itm.Id = Cnvrt_Dt[0]["FId"].ToString();
            }

            return itm;

        }

        public int FilesError(string query, string id)
        {
            FilesTableAdapter Flag_Ta = new FilesTableAdapter();
            Flag_Ta.Update_FilesError(query, long.Parse(id));
            return 1;

        }

        public void InsertFile(Stream Data)
        {
            string body = new StreamReader(Data).ReadToEnd();
            NameValueCollection nvc = HttpUtility.ParseQueryString(body);

            string FileName = nvc[0].ToString();
            string UserId = nvc[1].ToString();


            ServiceTableAdapter Ta = new ServiceTableAdapter();
            FilesTableAdapter FTa = new FilesTableAdapter();

            int RetId = int.Parse(FTa.Insert_File(FileName, int.Parse(UserId), 0, 0).ToString());

            MyDB.DataTable1DataTable Dt2 = Ta.Select_User_Directory(int.Parse(UserId));
            for (int i = 0; i < Dt2.Rows.Count; i++)
            {
                Ta.Insert_Convert_Q(RetId, int.Parse(Dt2[i]["CtPtblId"].ToString()), int.Parse(UserId));
            }

        }


    }
    public class UserFileList
    {
        public string Filename { get; set; }
        public string Duration { get; set; }
        public string FileSize { get; set; }
        public string UserID { get; set; }
        public string Convert_Datetime_Insert { get; set; }
        public string Convert_Datetime_Start { get; set; }
        public string Convert_Datetime_Done { get; set; }
        public string Converted { get; set; }
        public string Deleted { get; set; }
        public string Flag_Datetime_Insert { get; set; }
        public string Flag_Datetime_Start { get; set; }
        public string Flag_Datetime_Done { get; set; }
        public string FlagDone { get; set; }
        public string Upload_Datetime_Insert { get; set; }
        public string Upload_Datetime_Start { get; set; }
        public string Upload_Datetime_Done { get; set; }
        public string Uploaded { get; set; }


    }
    public class UserDetails
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string CategoryId { get; set; }

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
    public class FlagQueue
    {
        public string QfId { get; set; }
        public string Filename { get; set; }
        public string ConvertDirectory { get; set; }
        public string SrcDirectory { get; set; }
        public string FileId { get; set; }
        public string Command { get; set; }
        public string FilenameSuffix { get; set; }
        public string Kind { get; set; }
        public List<UploadQueue> UploadQueue { get; set; }


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


    }
    public class UserFiles
    {
        public string Filename { get; set; }
        public string Duration { get; set; }
        public string FileSize { get; set; }
        public string UserID { get; set; }
        public string Error { get; set; }
        public string ErrorLog { get; set; }

        public List<UserFilesConvert> FilesCnvrt { get; set; }
        public List<UserFilesFlag> FilesFlg { get; set; }
        public List<UserFilesUpload> FilesUpld { get; set; }

    }
    public class UserFilesConvert
    {
        public string Convert_Datetime_Insert { get; set; }
        public string Convert_Datetime_Start { get; set; }
        public string Convert_Datetime_Done { get; set; }
        public string ProfileTitle { get; set; }
        public string Converted { get; set; }

    }
    public class UserFilesFlag
    {
        public string Flag_Datetime_Insert { get; set; }
        public string Flag_Datetime_Start { get; set; }
        public string Flag_Datetime_Done { get; set; }
        public string Flaged { get; set; }
        public string ProfileTitle { get; set; }
    }
    public class UserFilesUpload
    {
        public string Upload_Datetime_Insert { get; set; }
        public string Upload_Datetime_Start { get; set; }
        public string Upload_Datetime_Done { get; set; }
        public string Uploaded { get; set; }
        public string ServerIp { get; set; }

        public string ProfileTitle { get; set; }
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
    public class RepositoryFiles
    {
        public string Filename { get; set; }
        public string Thumbnail { get; set; }
        public string Id { get; set; }
        public string serverPath { get; set; }
        public string relativePath { get; set; }
        public string serverCode { get; set; }

    }
}
