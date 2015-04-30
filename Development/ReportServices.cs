using ReportTransfert.ReportService2010;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace ReportTransfert
{
    public class ReportServices
    {
        private ReportingService2010SoapClient _service = null;
        public const string REGISTRY_APPLICATIONNAME = "ReportTransfert";

        /// <summary>
        /// Initializes a new instance of ReportServices to retrieve reports,
        /// to download and upload reports, etc.
        /// </summary>
        /// <param name="reportServerUrl"></param>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        public ReportServices(string reportServerUrl, string login, string password, string domain)
        {
            string url = reportServerUrl;
            if (!url.EndsWith("/") && !url.EndsWith(".asmx"))
                url += "/ReportService2010.asmx";
            else if (!url.EndsWith(".asmx"))
                url += "ReportService2010.asmx";

            NetworkCredential user = String.IsNullOrEmpty(domain) ? new NetworkCredential(login, password) : new NetworkCredential(login, password, domain);

            var binding = new System.ServiceModel.BasicHttpBinding();
            binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
            binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Ntlm;
            binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
            binding.Security.Message.ClientCredentialType = System.ServiceModel.BasicHttpMessageCredentialType.UserName;
            binding.Security.Message.AlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Default;
            binding.MaxReceivedMessageSize = 65536999;
            binding.MaxBufferSize = 65536999;
            binding.SendTimeout = new TimeSpan(0, 0, 0, 20);
            binding.OpenTimeout = new TimeSpan(0, 0, 0, 20);
            binding.ReaderQuotas.MaxStringContentLength = 10000;
            binding.ReaderQuotas.MaxDepth = 10000;
            binding.ReaderQuotas.MaxArrayLength = 10000;

            var endpoint = new System.ServiceModel.EndpointAddress(url);

            _service = new ReportingService2010SoapClient(binding, endpoint);
            _service.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            _service.ClientCredentials.Windows.ClientCredential = user;
            _service.Open();
        }

        /// <summary>
        /// Returns a list with all reports
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<CatalogItem[]> GetItems(string path)
        {
            TrustedUserHeader header = new TrustedUserHeader();

            return await System.Threading.Tasks.Task.Factory.FromAsync<CatalogItem[]>(
                _service.BeginListChildren(header, path, true, null, null),
                (ar) =>
                {
                    CatalogItem[] items = null;
                    _service.EndListChildren(ar, out items);
                    return items;
                });
        }

        /// <summary>
        /// Returns the content of specified file
        /// </summary>
        /// <param name="itemPath"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<byte[]> GetResourceContents(string itemPath)
        {
            TrustedUserHeader header = new TrustedUserHeader();

            return await System.Threading.Tasks.Task.Factory.FromAsync<byte[]>(
                _service.BeginGetItemDefinition(header, itemPath, null, null),
                (ar) =>
                {
                    byte[] data;
                    _service.EndGetItemDefinition(ar, out data);
                    return data;
                });
        }

        /// <summary>
        /// Upload the specified data content to a remote report (parent/name).
        /// </summary>
        /// <param name="dataType">Must be "Report" to upload a report content</param>
        /// <param name="sourceFile"></param>
        /// <param name="parent"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task CreateCatalogItem(string dataType, FileInfo sourceFile, DirectoryInfo relativeTo, string remoteParent, byte[] data)
        {
            TrustedUserHeader header = new TrustedUserHeader();
            Property prop1 = new Property() { Name = "MimeType", Value = MimeTypes.MimeTypeMap.GetMimeType(sourceFile.Extension) };
            string filenameWithoutExtension = sourceFile.Name.Substring(0, Convert.ToInt32(sourceFile.Name.Length - sourceFile.Extension.Length));

            string parent = await this.CreateSubFolders(sourceFile.FullName, relativeTo.FullName, remoteParent);

            await System.Threading.Tasks.Task.Factory.FromAsync(
                _service.BeginCreateCatalogItem(header, dataType, filenameWithoutExtension, parent, true, data, String.IsNullOrEmpty(prop1.Value) ? null : new Property[] { prop1 }, null, null)
                ,
                (ar) =>
                {
                    CatalogItem catalogItem;
                    Warning[] warning;
                    _service.EndCreateCatalogItem(ar, out catalogItem, out warning);
                    return data;
                });
        }

        /// <summary>
        /// Creates all subfolders to store the filename in a parent folder, relative to a base folder.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="relativeToFolder"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<string> CreateSubFolders(string filename, string relativeToFolder, string parent)
        {
            TrustedUserHeader header = new TrustedUserHeader();

            string[] folders = this.GetRelativePathSplitted(filename, relativeToFolder, true);
            string parentCompleted = parent;

            foreach (string folder in folders)
            {

                await System.Threading.Tasks.Task.Factory.FromAsync(
                _service.BeginCreateFolder(header, folder, parentCompleted, null, null, null)
                ,
                (ar) =>
                {
                    try
                    {
                        CatalogItem catalogItem;
                        _service.EndCreateFolder(ar, out catalogItem);
                    }
                    catch (System.ServiceModel.FaultException ex)
                    {
                        if (ex.Message.Contains("Microsoft.ReportingServices.Diagnostics.Utilities.ItemAlreadyExistsException"))
                        {
                            // Bypass if the folder already exits
                        }
                        else
                        {
                            throw;
                        }
                    }
                });

                parentCompleted += "/" + folder;
            }

            return parentCompleted;
        }

        /// <summary>
        /// Download the specified report file
        /// </summary>
        /// <param name="itemPath"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<System.Xml.XmlDocument> GetReportDefinition(string itemPath)
        {
            TrustedUserHeader header = new TrustedUserHeader();

            return await System.Threading.Tasks.Task.Factory.FromAsync<System.Xml.XmlDocument>(
                _service.BeginGetItemDefinition(header, itemPath, null, null),
                (ar) =>
                {
                    byte[] data;
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    ServerInfoHeader result = _service.EndGetItemDefinition(ar, out data);
                    MemoryStream stream = new MemoryStream(data);

                    doc.Load(stream);
                    return doc;
                });
        }

        /// <summary>
        /// Returns the sourcefile relative to the folder.
        /// Example: GetRelativePath(@"c:\foo\bar\blop\blap.txt", @"c:\foo\bar\") => @"blop\blap.txt"
        /// </summary>
        /// <param name="sourcefile"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private string GetRelativePath(string sourcefile, string folder)
        {
            Uri pathUri = new Uri(sourcefile);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Returns the list of sub-folders and filename of sourcefile relative to the folder.
        /// Example: GetRelativePath(@"c:\foo\bar\blop\blap.txt", @"c:\foo\") => string[] { "bar", "blop", "blap.txt" }
        /// </summary>
        /// <param name="sourcefile"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private string[] GetRelativePathSplitted(string sourcefile, string folder)
        {
            return this.GetRelativePath(sourcefile, folder).Split('\\');
        }

        /// <summary>
        /// Returns the list of sub-folders and filename of sourcefile relative to the folder.
        /// Example: GetRelativePath(@"c:\foo\bar\blop\blap.txt", @"c:\foo\") => string[] { "bar", "blop" }
        /// </summary>
        /// <param name="sourcefile"></param>
        /// <param name="folder"></param>
        /// <param name="removeFileName"></param>
        /// <returns></returns>
        private string[] GetRelativePathSplitted(string sourcefile, string folder, bool removeFileName)
        {
            string[] result = this.GetRelativePath(sourcefile, folder).Split('\\');

            if (!removeFileName)
            {
                return result;
            }
            else
            {
                return result.Take(result.Length - 1).ToArray();
            }
        }

    }
}