using ReportTransfert.ReportService2010;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportTransfert.Data
{
    /// <summary>
    /// Description of a report
    /// </summary>
    public class Report
    {
        ReportServices _service = null;
        CatalogItem _catalogitem = null;
        private string _name = string.Empty;
        private DateTime _modifiedDate = DateTime.MinValue;
        private string _typeName = string.Empty;
        
        /// <summary>
        /// Initializes a new instance of Report
        /// </summary>
        /// <param name="service"></param>
        /// <param name="catalogitem"></param>
        public Report(ReportServices service, CatalogItem catalogitem)
        {
            _service = service;
            _catalogitem = catalogitem;
            this.Name = catalogitem.Path;
            this.TypeName = catalogitem.TypeName;
            this.ModifiedDate = catalogitem.ModifiedDate;            
        }

        /// <summary>
        /// Gets or sets the Name of report
        /// </summary>
        public string Name {get; set; }

        /// <summary>
        /// Gets to sets the date of report modifications
        /// </summary>
        public DateTime ModifiedDate {get; set; }
        
        /// <summary>
        /// Gets to sets the type of resource
        /// </summary>
        public string TypeName {get; set; }
       
        /// <summary>
        /// Download the current report and save it in the TargetFile
        /// </summary>
        /// <param name="targetFile"></param>
        public async Task<string> DownloadToFile(string targetFile)
        {
            if (this.IsDownloadable)
            {
                FileInfo file = new FileInfo(targetFile);
                if (!file.Directory.Exists)
                {
                    file.Directory.Create();
                }

                if (this.IsReport)
                {
                    System.Xml.XmlDocument doc = await _service.GetReportDefinition(_catalogitem.Path);
                    doc.Save(targetFile);
                }
                else
                {
                    byte[] data = await _service.GetResourceContents(_catalogitem.Path);
                    System.IO.File.WriteAllBytes(targetFile, data);
                }

                return targetFile;
            }

            return String.Empty;
        }

        /// <summary>
        /// Download the current report and save it in the TargetPath with the report name
        /// </summary>
        /// <param name="targetPath"></param>
        public async Task<string> DownloadToFolder(string targetPath)
        {
            if (this.IsDownloadable)
            {
                string filename = targetPath;

                if (filename.EndsWith("\\"))
                {
                    filename = filename.Remove(filename.Length - 1);
                }

                if (this.IsReport)
                {
                    filename += _catalogitem.Path.Replace('/', '\\') + ".rdl";
                }
                else
                {
                    filename += _catalogitem.Path.Replace('/', '\\');
                }

                await this.DownloadToFile(filename);

                return filename;
            }

            return string.Empty;
        }

        /// <summary>
        /// Upload the specified file to this folder
        /// </summary>
        /// <param name="sourceFile"></param>
        public async System.Threading.Tasks.Task UploadFileInThisFolder(string sourceFile)
        {
            FileInfo file = new FileInfo(sourceFile);
            byte[] data = System.IO.File.ReadAllBytes(sourceFile);

            string filenameWithoutExtension = file.Name.Substring(0, Convert.ToInt32(file.Name.Length - file.Extension.Length));

            await _service.CreateCatalogItem("Report", filenameWithoutExtension, _catalogitem.Path, data);
        }

        /// <summary>
        /// Returns true of the report is a report... or False if the report is a resource (image, ...)
        /// </summary>
        /// <remarks>
        /// See http://msdn.microsoft.com/en-us/library/reportservice2010.reportingservice2010.listitemtypes.aspx
        /// </remarks>
        public bool IsReport
        {
            get
            {
                if (_catalogitem.TypeName == "Report" || 
                    _catalogitem.TypeName == "LinkedReport")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Return True if this resource is downloadable
        /// </summary>
        public bool IsDownloadable
        {
            get 
            {
                if (_catalogitem.TypeName == "Component" ||
                    _catalogitem.TypeName == "DataSource" ||
                    _catalogitem.TypeName == "Model" ||
                    _catalogitem.TypeName == "LinkedReport" ||
                    _catalogitem.TypeName == "Report" ||
                    _catalogitem.TypeName == "Resource" ||
                    _catalogitem.TypeName == "DataSet")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Return True if this resource is a folder
        /// </summary>
        public bool IsFolder
        {
            get
            {
                if (_catalogitem.TypeName == "Folder")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
