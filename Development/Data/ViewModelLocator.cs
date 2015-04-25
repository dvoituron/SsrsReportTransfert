using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportTransfert.Data
{
    public class ViewModelLocator
    {

        /// <summary>
        /// Gets a reference to the Main ViewModel
        /// </summary>
        public MainViewModel Main
        {
            get
            {
                return new MainViewModel();
            }
        }

        /// <summary>
        /// Gets a reference to the Main ViewModel
        /// </summary>
        public FilesSelectionViewModel FilesSelection
        {
            get
            {
                return new FilesSelectionViewModel();
            }
        }
    }
}
