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

                if (this.IsReportResource)
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
                string extension = this.GetFileExtensionFromReportResource(this.ReportType);

                if (filename.EndsWith("\\"))
                {
                    filename = filename.Remove(filename.Length - 1);
                }

                filename += _catalogitem.Path.Replace('/', '\\') + extension;

                await this.DownloadToFile(filename);

                return filename;
            }

            return string.Empty;
        }

        /// <summary>
        /// Upload the specified file to this folder
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="relativeTo"></param>
        /// <param name="type"></param>
        public async System.Threading.Tasks.Task UploadFileInThisFolder(FileInfo sourceFile, DirectoryInfo relativeTo, ReportResource type)
        {
            byte[] data = System.IO.File.ReadAllBytes(sourceFile.FullName);

            await _service.CreateCatalogItem(this.GetReportResourceName(type), sourceFile, relativeTo, _catalogitem.Path, data);
        }
        /// <summary>
        /// Upload the specified file to this folder (based on the file extension)
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="relativeTo"></param>
        public async System.Threading.Tasks.Task UploadFileInThisFolder(FileInfo sourceFile, DirectoryInfo relativeTo)
        {
            await this.UploadFileInThisFolder(sourceFile, relativeTo, this.GetResourceTypeFromExtension(sourceFile));
        }
        
        /// <summary>
        /// Returns true of the report is a report... or False if the report is a resource (image, ...)
        /// </summary>
        /// <remarks>
        /// See http://msdn.microsoft.com/en-us/library/reportservice2010.reportingservice2010.listitemtypes.aspx
        /// </remarks>
        public bool IsReportResource
        {
            get
            {
                ReportResource reportType = this.ReportType;

                if (reportType == ReportResource.LinkedReport ||
                    reportType == ReportResource.DataSet ||
                    reportType == ReportResource.LinkedReport ||
                    reportType == ReportResource.Report ||
                    reportType == ReportResource.DataSource)
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
        /// Gets the current report resource type.
        /// </summary>
        public ReportResource ReportType
        {
            get
            {
                return this.GetReportResourceEnum(_catalogitem.TypeName);
            }
        }

        /// <summary>
        /// Returns Extension (with comma) from Report Resource Type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetFileExtensionFromReportResource(ReportResource type)
        {
            switch (type)
            {
                case ReportResource.Component:
                    return ".rdc";
                case ReportResource.DataSource:
                    return ".rds";
                case ReportResource.Model:
                    return ".rdm";
                case ReportResource.LinkedReport:
                    return ".rdr";
                case ReportResource.Report:
                    return ".rdl";
                case ReportResource.Resource:
                    return ".rdx";
                case ReportResource.DataSet:
                    return ".rsd";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Returns the ReportResource from File Extension
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public ReportResource GetResourceTypeFromExtension(FileInfo file)
        {
            return this.GetResourceTypeFromExtension(file.Extension);
        }

        /// <summary>
        /// Returns the ReportResource from File Extension
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public ReportResource GetResourceTypeFromExtension(string fileExtension)
        {
            switch (fileExtension)
            {
                case ".rdc":
                    return ReportResource.Component;
                case ".rds":
                    return ReportResource.DataSource;
                case ".rdm":
                    return ReportResource.Model;
                case ".rdr":
                    return ReportResource.LinkedReport;
                case ".rdx":
                    return ReportResource.Resource;
                case ".rdl":
                    return ReportResource.Report;
                case ".rsd":
                    return ReportResource.DataSet;
                default:
                    return ReportResource.Unknown;
            }
        }

        /// <summary>
        /// Return True if this resource is downloadable
        /// </summary>
        public bool IsDownloadable
        {
            get 
            {
                ReportResource reportType = this.GetReportResourceEnum(_catalogitem.TypeName);

                if (reportType == ReportResource.Component ||
                    reportType == ReportResource.DataSource ||
                    reportType == ReportResource.Model ||
                    reportType == ReportResource.LinkedReport ||
                    reportType == ReportResource.Report ||
                    reportType == ReportResource.Resource ||
                    reportType == ReportResource.DataSet)
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
        /// Returns the report type name.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetReportResourceName(ReportResource type)
        {
            switch (type)
            {
                case ReportResource.Component:
                    return "Component";
                case ReportResource.DataSource:
                    return "DataSource";
                case ReportResource.Model:
                    return "Model";
                case ReportResource.LinkedReport:
                    return "LinkedReport";
                case ReportResource.Report:
                    return "Report";
                case ReportResource.Resource:
                    return "Resource";
                case ReportResource.DataSet:
                    return "DataSet";
                default:
                    return "Resource";
            }
        }

        /// <summary>
        /// Returns the Type of Report from report type
        /// </summary>
        /// <param name="reportTypeName"></param>
        /// <returns></returns>
        private ReportResource GetReportResourceEnum(string reportTypeName)
        {
            switch (reportTypeName)
            {
                case "Component":
                    return ReportResource.Component;
                case "DataSource":
                    return ReportResource.DataSource;
                case "Model":
                    return ReportResource.Model;
                case "LinkedReport":
                    return ReportResource.LinkedReport;
                case "Report":
                    return ReportResource.Report;
                case "Resource":
                    return ReportResource.Resource;
                case "DataSet":
                    return ReportResource.DataSet;
                default:
                    return ReportResource.Unknown;
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

    /// <summary>
    /// Type of report resource
    /// </summary>
    public enum ReportResource
    {
        Unknown,
        Component,
        DataSource,
        Model,
        LinkedReport,
        Report,
        Resource,
        DataSet
    }
}
