using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Data.SqlClient;
using System.Web.Caching;
using System.Web.Hosting;
using Wutnu.Common;
using Wutnu.Repo;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Wutnu.Infrastructure
{
    /// <summary>
    /// Class WutVirtualPathProvider
    /// </summary>
    public class WutVirtualPathProvider : VirtualPathProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WutVirtualPathProvider" /> class.
        /// </summary>
        public WutVirtualPathProvider() : base()
        {
        }

        /// <summary>
        /// Gets a value that indicates whether a file exists in the virtual file system.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual file.</param>
        /// <returns>true if the file exists in the virtual file system; otherwise, false.</returns>
        public override bool FileExists(string virtualPath)
        {
            if (virtualPath.StartsWith("/Blob/Report/"))
                return true;

            return base.FileExists(virtualPath);
        }
        /// <summary>
        /// Gets a virtual file from the virtual file system.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual file.</param>
        /// <returns>A descendent of the <see cref="T:System.Web.Hosting.VirtualFile" /> class that represents a file in the virtual file system.</returns>
        public override VirtualFile GetFile(string virtualPath)
        {
            if (virtualPath.StartsWith("/Blob/Report/"))
                return new BlobVirtualFile(virtualPath);

            return base.GetFile(virtualPath);
        }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            var version = base.GetFileHash(virtualPath, virtualPathDependencies);
            if (virtualPath.StartsWith("/Blob/Report/"))
            {
                //get the widget record, save the EF Version into cache as this file's current signature
                //when the record updates, the file will be retrieved and recompiled
                //TODO: setup table for report defs, retrieve those into cache
                //using (var io = new Data.WutNuContext())
                //{
                //    var s = VirtualPathUtility.GetFileName(virtualPath);
                //    var arr = io. .Single(w => w.URL == s).Version;
                //    version = Encoding.UTF8.GetString(arr);
                //}

                //TODO: remove after debugging is complete.
                version = DateTime.Now.Ticks.ToString();
            }
            return version;
        }
        /// <summary>
        /// Creates a cache dependency based on the specified virtual paths.
        /// </summary>
        /// <param name="virtualPath">The path to the primary virtual resource.</param>
        /// <param name="virtualPathDependencies">An array of paths to other resources required by the primary virtual resource.</param>
        /// <param name="utcStart">The UTC time at which the virtual resources were read.</param>
        /// <returns>A <see cref="T:System.Web.Caching.CacheDependency" /> object for the specified virtual resources.</returns>
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            return null;
        }
    }

    public static class CompiledReports
    {
        public static Dictionary<string, string> Reports;
        static CompiledReports()
        {
            Reports = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Class BlobVirtualFile
    /// </summary>
    public class BlobVirtualFile : VirtualFile
    {
        private readonly string _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobVirtualFile" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public BlobVirtualFile(string path)
            : base(path)
        {
            _path = path;
        }

        /// <summary>
        /// When overridden in a derived class, returns a read-only stream to the virtual resource.
        /// </summary>
        /// <returns>A read-only stream to the virtual file.</returns>
        public override Stream Open()
        {
            byte[] data;

            var fileName = VirtualPathUtility.GetFileName(_path);
            try
            {
                var context = HttpContext.Current;
                if (!Settings.LocalReports)
                {
                    var container = WutStorage.GetContainer("reports");
                    var blobFile = (CloudBlockBlob)WutStorage.GetBlob(container, fileName);
                    blobFile.FetchAttributes();
                    data = new byte[blobFile.Properties.Length];
                    blobFile.DownloadToByteArray(data, 0);
                }
                else
                {
                    var filepath = context.Server.MapPath("/reportslocal/") + fileName;
                    using (var file = File.Open(filepath, FileMode.Open))
                    {
                        data = new byte[file.Length];
                        file.Read(data, 0, (int)file.Length);
                        file.Close();
                    }
                }
                return new MemoryStream(data);
            }
            catch (Exception ex)
            {
                var msg=String.Format("Error retrieving {0} from {1}", fileName, ((!Settings.LocalReports) ? "blob" : "local storage"));
                throw new Exception(msg, ex);
            }
        }
    }
}