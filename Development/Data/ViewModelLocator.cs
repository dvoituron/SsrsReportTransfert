using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportTransfert.Data
{
    public class ViewModelLocator
    {
        private static MainViewModel _mainViewModel = null;
        private static FilesSelectionViewModel _filesSelection;

        /// <summary>
        /// Gets a reference to the Main ViewModel
        /// </summary>
        public MainViewModel Main
        {
            get
            {
                if (ViewModelLocator._mainViewModel == null)
                {
                    _mainViewModel = new MainViewModel();
                }
                return _mainViewModel;
            }
        }

        /// <summary>
        /// Gets a reference to the Main ViewModel
        /// </summary>
        public FilesSelectionViewModel FilesSelection
        {
            get
            {
                if (ViewModelLocator._filesSelection == null)
                {
                    _filesSelection = new FilesSelectionViewModel();
                }
                return _filesSelection;
            }
        }

        /// <summary>
        /// Gets a reference to the current locator
        /// </summary>
        public static ViewModelLocator Locator
        {
            get
            {
                return System.Windows.Application.Current.Resources["Locator"] as ViewModelLocator;
            }
        }
    }
}
