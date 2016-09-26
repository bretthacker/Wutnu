using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace BlobCopy
{
    class Program
    {
        static void Main(string[] args)
        {
            
            if (args.Length == 0)
            {
                Info();
                return;
            }
            var settings = Properties.Settings.Default;
            using (var blob = new BlobUtil(settings.ApiKey, settings.ApiUrl, Environment.CurrentDirectory))
            {
                blob.FileChanged += Blob_FileChanged;
                var value = "";
                if (args.Length == 2)
                {
                    value = args[1];
                }
                IEnumerable<FilePoco> res;

                switch (args[0])
                {
                    case "put":
                        blob.Upload(value);
                        CheckErrorMessage(blob);

                        while (!blob.Done)
                        {
                        }
                        break;

                    case "list":
                        res = blob.ListFiles();
                        Console.WriteLine("");
                        CheckErrorMessage(blob);

                        //write files
                        WriteList(res);
                        break;

                    case "get":
                        blob.Download(value);
                        CheckErrorMessage(blob);
                        while (!blob.Done)
                        {
                        }
                        break;

                    case "delete":
                        res = blob.Delete(value);
                        Console.WriteLine("");
                        CheckErrorMessage(blob);

                        Console.WriteLine("");
                        //write files
                        WriteList(res);
                        break;

                    default:
                        Info();
                        break;
                }
            }
            Console.Out.Flush();
        }

        private static void CheckErrorMessage(BlobUtil blob)
        {
            if (blob.Message!=null && blob.Message.Length>0)
            {
                Console.WriteLine(blob.Message);
            }
        }

        private static void WriteList(IEnumerable<FilePoco> data)
        {
            if (data == null) return;

            Console.WriteLine(GetFileLineItem(new FilePoco
            {
                FileName = "File Name",
                Path = "Path",
                FileType = "File Type",
                Length = "Length"
            }));
            Console.WriteLine(new String('-', 90));
            foreach (var item in data)
            {
                Console.WriteLine(GetFileLineItem(item));
            }
        }

        private static string GetFileLineItem(FilePoco item)
        {
            StringBuilder res = new StringBuilder();
            res.Append(string.Join("", item.FileName.Take(50)));
            var x = 46 - item.FileName.Length;
            res.Append(new string(' ', (0>x) ? 0 : x));

            res.Append(string.Join("", item.FileType.Take(25)));
            x = 26 - item.FileType.Length;
            res.Append(new string(' ', (0>x) ? 0 : x));

            res.Append(item.Length);
            res.Append(new string(' ', 15 - item.Length.Length));

            return res.ToString();
        }
        private static void Blob_FileChanged(object sender, CopyEventArgs e, string message)
        {
            switch(message)
            {
                case "downloadProgress":
                    Console.Write("\r{0} {1}", e.PercentComplete + "% ", new string('-', Convert.ToInt16(80 * (e.PercentComplete * .01))));
                    break;

                case "downloadCompleted":
                    Console.Write("\r{0} {1}", "100% ", new string('-', 80));
                    Console.WriteLine("");
                    Console.WriteLine("File download completed.");
                    break;

                case "uploadProgress":
                    Console.Write("\r{0} {1}", e.PercentComplete + "% ", new string('-', Convert.ToInt16(80 * (e.PercentComplete * .01))));
                    break;

                case "uploadCompleted":
                    Console.WriteLine("");
                    Console.Write("\r{0} {1}", "100% ", new string('-', 80));
                    Console.WriteLine("File upload completed.");
                    break;

                case "transferError":
                    Console.WriteLine("");
                    Console.WriteLine(e.ErrorMessage);
                    Console.WriteLine("File transfer operation terminated.");
                    break;
            }
        }

        static void Info()
        {
            Console.Clear();
            Console.WriteLine("BlobCopy Utility");
            Console.WriteLine("");
            Console.WriteLine("This utility fetches a SAS token with authorization to upload, list, and download files from");
            Console.WriteLine("an assigned container in Azure Blob Storage.");
            Console.WriteLine("");
            Console.WriteLine("SETTINGS (in App.config)");
            Console.WriteLine("   ApiUrl: the server URL used by the utility to retrieve authorization tokens.");
            Console.WriteLine("   ApiKey: the key used by the utility to authenticate calls to retrieve auth tokens.");
            Console.WriteLine("");
            Console.WriteLine("USAGE");
            Console.WriteLine("   blobcopy put <path_to_file>");
            Console.WriteLine("");
            Console.WriteLine("   blobcopy list");
            Console.WriteLine("       NOTE: files are case-sensitive.");
            Console.WriteLine("");
            Console.WriteLine("   blobcopy get <filename in blob>");
            Console.WriteLine("");
            Console.WriteLine("   blobcopy delete <filename in blob>");
        }
    }
}
