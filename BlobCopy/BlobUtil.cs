using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Diagnostics;
using Wutnu.Common.ErrorMgr;
using Wutnu.Common;
using Microsoft.WindowsAzure.Storage.DataMovement;
using System.Threading;

namespace BlobCopy
{
    public delegate void FileStatusHandler(object sender, CopyEventArgs e, string message);

    public class BlobUtil: IDisposable
    {
        public event FileStatusHandler FileChanged;
        public bool Done;
        public string Message;
        private string _apiUrl;
        private string _currDirectory;
        private WebClient _web;

        protected virtual void OnChanged(object sender, CopyEventArgs e, string message)
        {
            FileChanged(this, e, message);
        }

        public BlobUtil(string apiKey, string apiUrl, string currDirectory)
        {
            try
            {
                _apiUrl = apiUrl;
                _currDirectory = currDirectory;
                _web = new WebClient();

                //ignore certificate warnings while in development
                if (apiUrl.IndexOf("localhost") > -1) ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                _web.Headers.Set("apikey", apiKey);
                _web.BaseAddress = apiUrl;
                _web.DownloadProgressChanged += _web_DownloadProgressChanged;
                _web.DownloadFileCompleted += _web_DownloadFileCompleted;
            }
            catch (Exception ex)
            {
                ListError(ex);
            }
        }
        public IEnumerable<FilePoco> Delete(string fileName)
        {
            List<FilePoco> res = new List<FilePoco>();

            try
            {
                _web.QueryString.Add("path", fileName);
                var data = _web.UploadString("DeleteBlob", fileName);
                var deleteSuccess = _web.ResponseHeaders["FileDeleted"];
                Message = (Convert.ToBoolean(deleteSuccess)) ? "File deleted successfully." : "File not deleted or file not found.";
                var coll = JsonConvert.DeserializeObject<ICollection<dynamic>>(data);
                return coll.Select(FilePoco.GetPocoFromDynamic);
            }
            catch (Exception ex)
            {
                ListError(ex);
                return null;
            }
        }

        public void Upload(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_currDirectory, fileName);

                _web.QueryString.Add("path", fileName);
                var token = _web.DownloadString("GetUploadToken");
                _web.QueryString.Clear();

                var tokenEl = token.Split('?');
                Uri blobUri = new Uri(tokenEl[0]);
                FileInfo f = new FileInfo(filePath);
                long bytesToTransfer = f.Length;
                var contentType = System.Web.MimeMapping.GetMimeMapping(filePath);

                // Create credentials with the SAS token.
                StorageCredentials credentials = new StorageCredentials(tokenEl[1]);

                // Setup the number of the concurrent operations
                TransferManager.Configurations.ParallelOperations = 64;

                // Setup the transfer context and track the upoload progress
                TransferContext context = new TransferContext();
                context.ProgressHandler = new Progress<TransferStatus>((progress) =>
                {
                    int p = Convert.ToInt32(progress.BytesTransferred / bytesToTransfer);
                    _tc_UploadProgressChanged(progress, p);
                });
                
                context.FileTransferred += _tc_FileTransferred;

                var options = new UploadOptions
                {
                    ContentType = contentType
                };

                var blob = new CloudBlockBlob(blobUri,credentials);
                blob.Properties.ContentType = contentType;
                
                // Upload a local blob
                var task = TransferManager.UploadAsync(filePath, blob, null, context, CancellationToken.None);
                task.Wait();
            }
            catch (Exception ex)
            {
                EventError(ex);
            }
        }

        public void Download(string path)
        {
            try
            {
                _web.QueryString.Add("path", path);
                var token = _web.DownloadString("GetDownloadToken");
                _web.QueryString.Clear();
                _web.DownloadFileAsync(new Uri(token), Path.Combine(_currDirectory, Path.GetFileName(path)));
            }
            catch (Exception ex)
            {
                EventError(ex);
            }
        }

        public IEnumerable<FilePoco> ListFiles()
        {
            List<FilePoco> res = new List<FilePoco>();

            try
            {
                var data = _web.DownloadString("GetList");
                var coll = JsonConvert.DeserializeObject<ICollection<dynamic>>(data);
                return coll.Select(FilePoco.GetPocoFromDynamic);
            }
            catch (Exception ex)
            {
                ListError(ex);
                return null;
            }
        }
        private string ErrorString(Exception ex)
        {
            Message = string.Format("Sorry, an error occured: {0}. Details are in the Windows Event Log.", ex.GetBaseException().Message);
            
            return Message;
        }
        private string ListError(Exception ex)
        {
            Logging.WriteToAppLog(ex.Message, EventLogEntryType.Error, ex);
            return ErrorString(ex);
        }

        #region webevents
        private void EventError(Exception ex)
        {
            Logging.WriteToAppLog(ex.GetBaseException().Message, EventLogEntryType.Error, ex);
            Done = true;
            OnChanged(ex, CopyEventArgs.Error(ErrorString(ex)), "transferError");
        }
        private void _tc_FileTransferred(object sender, TransferEventArgs e)
        {
            Done = true;
            OnChanged(e, null, "uploadCompleted");
        }
        private void _tc_UploadProgressChanged(TransferStatus e, int progress)
        {
            OnChanged(e, CopyEventArgs.Args(progress), "uploadProgress");
        }
        private void _web_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Done = true;
            OnChanged(sender, null, "downloadCompleted");
        }
        private void _web_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnChanged(sender, CopyEventArgs.Args(e.ProgressPercentage), "downloadProgress");
        }
        #endregion

        public void Dispose()
        {
            _web.Dispose();
        }
    }

    public class CopyEventArgs: EventArgs
    {
        public int PercentComplete { get; set; }
        public string ErrorMessage { get; set; }

        public CopyEventArgs(int percentComplete)
        {
            PercentComplete = percentComplete;
        }
        public static CopyEventArgs Args(int percentComplete)
        {
            return new CopyEventArgs(percentComplete);
        }
        public static CopyEventArgs Error(string errorMessage)
        {
            return new CopyEventArgs(0)
            {
                ErrorMessage = errorMessage
            };
        }
    }

    public class FilePoco
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Length { get; set; }
        public string FileType { get; set; }

        public FilePoco()
        {
            FileName = "";
            Path = "";
            Length = "";
            FileType = "";
        }

        public static FilePoco GetPocoFromDynamic(dynamic item)
        {
            string container = item.Container.Name;
            string uri = item.Uri;
            var items = uri.Split(new string[] { container }, StringSplitOptions.None);
            string path = items[1];
            path = path.Substring(0, path.Length - item.Name.Value.Length);

            return new FilePoco
            {
                FileName = item.Name,
                Path = path,
                FileType = item.Properties.ContentType,
                Length = Math.Round(Convert.ToDecimal(item.Properties.Length) / 1024, 2).ToString()
            };
        }
    }
}
