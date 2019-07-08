using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using McService.MyDBTableAdapters;

namespace McService
{
    public partial class Uploader : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            int UserId = 8;
            UserId= int.Parse(Request.QueryString["UserId"].ToString());
           
            double qqtotalfilesize = double.Parse(Request.Params["qqtotalfilesize"]);

            ServiceTableAdapter Ta = new ServiceTableAdapter();
            MyDB.DataTable1DataTable Dt= Ta.Select_DirForUpload(UserId);



            Response.ContentType = "application/json";
            int bytesRead = 0;
            string result = "{\"success\":true";
            string path = Dt[0]["SrcDirectory"].ToString()+DateTime.Now.ToString("yyyyMMdd");
            string saveLocation = string.Empty;
            string fileName = string.Empty;
            string RetfileName = string.Empty;

            const int length = 4096;
           
            Byte[] buffer = new Byte[length];
            double ReadBuffer = 0;

            if (Request.Params["fupfile"] != null)//This works with Chrome/FF/Safari
            {
                try
                {
                    fileName = Request.Params["fupfile"];

                    RetfileName = Request.Params["fupfile"];
                    //saveLocation = Server.MapPath(path) + "\\" + fileName;
                    if (!System.IO.Directory.Exists(path))
                        System.IO.Directory.CreateDirectory(path);
                    saveLocation = path + "\\" + fileName;

                    using (System.IO.FileStream fileStream = new System.IO.FileStream(saveLocation, System.IO.FileMode.Create))
                    {
                        do
                        {
                            bytesRead = Request.InputStream.Read(buffer, 0, length);
                            fileStream.Write(buffer, 0, bytesRead);
                            ReadBuffer += bytesRead;
                        }
                        while (bytesRead > 0);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // log error hinting to set the write permission of ASPNET or the identity accessing the code
                    result = result.Replace("true", "false, \"error\":" + ex.Message + " " + ex.InnerException);
                }
            }
            else//IE
            {
                try
                {
                    //if (Request.Files[0].ContentLength < 102400000)
                    //{
                    fileName = System.IO.Path.GetFileName(Request.Files[0].FileName);

                    RetfileName = System.IO.Path.GetFileName(Request.Files[0].FileName);
                        //saveLocation = Server.MapPath(path) + "\\" + fileName;
                        if (!System.IO.Directory.Exists(path))
                            System.IO.Directory.CreateDirectory(path);
                        saveLocation = path + "\\" + fileName;

                        using (System.IO.FileStream fileStream = new System.IO.FileStream(saveLocation, System.IO.FileMode.Create))
                        {
                            do
                            {
                                bytesRead = Request.Files[0].InputStream.Read(buffer, 0, length);
                                fileStream.Write(buffer, 0, bytesRead);
                                ReadBuffer += bytesRead;
                            }
                            while (bytesRead > 0);
                        }
                    //}
                    //else
                    //    result = result.Replace("true", "false, \"error\":File size");
                }
                catch (Exception ex)
                {
                    // log error hinting to set the write permission of ASPNET or the identity accessing the code
                    result = result.Replace("true", "false, \"error\":\"" + ex.Message + " " + ex.InnerException + "\"");
                }
            }

            if (qqtotalfilesize == ReadBuffer)
            {
                result += ", \"fileName\": \"" + RetfileName + "\"}";

                FilesTableAdapter FTa = new FilesTableAdapter();
                int RetId = int.Parse(FTa.Insert_File(DateTime.Now.ToString("yyyyMMdd") + "\\" + fileName, UserId, 0, 0, DateTime.Now.ToString("yyyyMMdd") + "\\" + Request.Files[0].FileName).ToString());
                MyDB.DataTable1DataTable Dt2 = Ta.Select_User_Directory(UserId);
                for (int i = 0; i < Dt2.Rows.Count; i++)
                {
                    Ta.Insert_Convert_Q(RetId, int.Parse(Dt2[i]["CtPtblId"].ToString()), UserId);
                }
            }

            Response.Write(result);
        }
    }
}
