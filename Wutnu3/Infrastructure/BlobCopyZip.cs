using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Xml;

namespace Wutnu.Web.Infrastructure
{
    public static class BlobCopyZip
    {
        public static void InitZip(string dirPath)
        {
            var folderPath = Path.Combine(dirPath, "Files/BlobCopy");
            var folder = Directory.EnumerateFiles(folderPath);

            using (var fileStream = new FileStream(Path.Combine(folderPath, "BlobCopy.zip"), FileMode.Create))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, false))
                {
                    foreach (var file in folder)
                    {
                        var fileName = Path.GetFileName(file);
                        if (fileName == "api.config.txt") continue;
                        if (fileName == "BlobCopy.exe.config.txt")
                            fileName = "BlobCopy.exe.config";

                        using (var stream = File.Open(file, FileMode.Open))
                        {
                            var item = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                            using (var itemStream = item.Open())
                            {
                                stream.CopyTo(itemStream);
                            }
                        }
                    }
                }
            }
        }

        public static MemoryStream SetupZip(string dirPath, string apiKey, string apiUrl)
        {
            var configFile = GetConfig(dirPath, apiKey, apiUrl);
            using (var zipFile = File.Open(Path.Combine(dirPath, "BlobCopy.zip"), FileMode.Open))
            {
                Stream zipCopy = new MemoryStream();
                zipFile.CopyTo(zipCopy);

                using (var archive = new ZipArchive(zipCopy, ZipArchiveMode.Update, true))
                {
                    var item = archive.CreateEntry("api.config", CompressionLevel.Fastest);

                    using (var streamWriter = new StreamWriter(item.Open()))
                    {
                        streamWriter.Write(configFile);
                    }
                    return (zipCopy as MemoryStream);
                }
            }
        }

        private static string GetConfig(string dirPath, string apiKey, string apiUrl)
        {
            var file = new XmlDocument();
            file.Load(Path.Combine(dirPath, "api.config.txt"));

            var key = file.SelectSingleNode("//add[@key='ApiKey']");
            key.Attributes["value"].Value = apiKey;
            key = file.SelectSingleNode("//add[@key='ApiUrl']");
            key.Attributes["value"].Value = apiUrl;
            return file.OuterXml;
        }
    }
}