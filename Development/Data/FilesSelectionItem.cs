using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportTransfert.Data
{
    public class FilesSelectionItem
    {
        /// <summary>
        /// Initializes a new instance of FilesSelectionItem
        /// </summary>
        /// <param name="file"></param>
        /// <param name="baseFolder"></param>
        public FilesSelectionItem(FileInfo file, DirectoryInfo baseFolder)
        {
            this.File = file;
            this.BaseFolder = baseFolder;
        }

        /// <summary>
        /// Gets the defined file
        /// </summary>
        public FileInfo File { get; private set; }

        /// <summary>
        /// Gets the defined folder base
        /// </summary>
        public DirectoryInfo BaseFolder { get; private set; }

        /// <summary>
        /// Gets the name of file relative to BaseFolder
        /// </summary>
        public string RelativeName
        {
            get
            {
                return Report.GetRelativePath(this.File.FullName, this.BaseFolder.FullName);
            }
        }

        /// <summary>
        /// Gets the type of resource (file)
        /// </summary>
        public string Type
        {
            get
            {
                return Report.GetReportResourceName(Report.GetResourceTypeFromExtension(this.File));
            }
        }

        /// <summary>
        /// Gets the last modified date/time
        /// </summary>
        public DateTime Modified
        {
            get
            {
                return this.File.LastWriteTime;
            }
        }
    }
}
