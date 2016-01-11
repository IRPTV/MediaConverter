using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace McService
{

    [ServiceContract]

    public interface IMc
    {
        #region Users
        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "user/list")]
        List<UserDetails> UsersList();

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "user/{Username}")]
        string UserDetail(string Username);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "userlogin")]
        UserDetails LoginUser(Stream Data);

        #endregion

        #region Files

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "files/user/{UserId}")]
        List<UserFiles> UserFiles(string UserId);



        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "files/convert/{TopCount}/{Converted}/{ProfileID}/{ServerCode}")]
        List<ConvertQueue> GetConvertQueue(string TopCount, string Converted, string ProfileID, string ServerCode);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "files/convert/{TopCount}/{Converted}")]
        List<ConvertQueue> GetConvertQueueAll(string TopCount, string Converted);



        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "files/convert/{ConvertId}/start")]
        int SetConvertQueueStart(string ConvertId);
        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "files/convert/{ConvertId}/delete")]
        void SetConvertQueuedelete(string ConvertId);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "files/convert/{ConvertId}/undone")]
        void SetConvertQueueUnDone(string ConvertId);
        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "files/convert/{ConvertId}/done")]
        int SetConvertQueueDone(string ConvertId);


        [WebInvoke(Method = "GET",
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Bare,
           UriTemplate = "files/flag/{TopCount}/{Flaged}/{ServerCode}")]
        List<FlagQueue> GetFlagQueue(string TopCount, string Flaged, string ServerCode);


        [WebInvoke(Method = "GET",
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Bare,
           UriTemplate = "files/flag/{TopCount}/{Flaged}")]
        List<FlagQueue> GetFlagQueueDefaultServer(string TopCount, string Flaged);



        [WebInvoke(Method = "GET",
          ResponseFormat = WebMessageFormat.Json,
          BodyStyle = WebMessageBodyStyle.Bare,
          UriTemplate = "files/flag/{FlagId}/start")]
        int SetFlagQueueStart(string FlagId);

        [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.Bare,
        UriTemplate = "files/flag/{FlagId}/done")]
        int SetFlagQueueDone(string FlagId);

        [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.Bare,
        UriTemplate = "files/upload/{TopCount}/{ServerCode}")]
        List<UploadQueue> GetUploadQueue(string TopCount, string ServerCode);



        [WebInvoke(Method = "GET",
      ResponseFormat = WebMessageFormat.Json,
      BodyStyle = WebMessageBodyStyle.Bare,
      UriTemplate = "files/upload/{TopCount}/{Ip}/{ServerCode}")]
        List<UploadQueue> GetUploadQueueServer(string TopCount, string Ip, string ServerCode);


        [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.Bare,
        UriTemplate = "files/upload/{UploadId}/start")]
        int SetUploadQueueStart(string UploadId);


        [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.Bare,
        UriTemplate = "files/upload/{UploadId}/done")]

        int SetUploadQueueDone(string UploadId);


        [OperationContract]
        [WebInvoke(Method = "*", UriTemplate = "File/upload")]
        void UploadFile(Stream fileContents);
        #endregion
        
        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "files/Logo/{Count}/{ServerCode}")]
        List<LogoQueue> GetLogoQueue(string Count, string ServerCode);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "files/Logo/{LogoID}/Done/{Duration}")]
        int SetLogoDone(string LogoID,string Duration);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "files/Search/{Count}/files?query={query}&start={start}&End={End}")]
        List<RepositoryFiles> SearchFiles(string Count,string query,string start,string End);


        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "files/Error?query={query}&id={id}")]
        int FilesError(string query,string id);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "files/{id}")]
        RepositoryFiles SearchFilesId(string id);



        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "files/insert")]
        void InsertFile(Stream Data);
    }
}
