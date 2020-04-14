using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
            using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
namespace API_assignment_v2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                new UploadVideo().Run(txtTitle.Text, txt_Description.Text, txt_DisplayStatus.Text).Wait();
                
            }
            catch (AggregateException ex)
            {
                foreach (var er in ex.InnerExceptions)
                {
                   Console.WriteLine("Error: " + er.Message);
                }
            }
        }

    }

    public class UploadVideo
  {
    [STAThread]

        public async Task Run(string txtTitle, string txt_Description, string txt_DisplayStatus)
    {
      UserCredential credential;
      using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
      {
        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.Load(stream).Secrets,
            // This OAuth 2.0 access scope allows an application to upload files to the
            // authenticated user's YouTube channel, but doesn't allow other types of access.
            new[] { YouTubeService.Scope.YoutubeUpload },
            "user",
            CancellationToken.None
        );
      }

      var youtubeService = new YouTubeService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
      });

      var video = new Video();
      video.Snippet = new VideoSnippet();
      video.Snippet.Title = txtTitle;
      video.Snippet.Description = txt_Description;
      video.Snippet.Tags = new string[] { "tag1", "tag2" };
      video.Snippet.CategoryId = "22"; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
      video.Status = new VideoStatus();
      video.Status.PrivacyStatus = txt_DisplayStatus; // or "private" or "public"
      var filePath = @"C:\Users\zach\source\repos\API assignment v2\API assignment v2\bin\Debug\video0.mp4"; // Replace with path to actual movie file.

      using (var fileStream = new FileStream(filePath, FileMode.Open))
      {
        var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
        videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
        videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;

        await videosInsertRequest.UploadAsync();
      }
    }

    void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
    {
      switch (progress.Status)
      {
        case UploadStatus.Uploading:
          Console.WriteLine("{0} bytes sent.", progress.BytesSent);
          break;

        case UploadStatus.Failed:
          Console.WriteLine("An error prevented the upload from completing.\n{0}", progress.Exception);
          break;
      }
    }

    void videosInsertRequest_ResponseReceived(Video video)
    {
      Console.WriteLine("Video id '{0}' was successfully uploaded.", video.Id);
    }
  }
}
        
    



