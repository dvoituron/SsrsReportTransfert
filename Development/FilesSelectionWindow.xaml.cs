using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ReportTransfert
{
    /// <summary>
    /// Interaction logic for FilesSelectionWindow.xaml
    /// </summary>
    public partial class FilesSelectionWindow : Window
    {
        /// <summary>
        /// 
        /// </summary>
        public FilesSelectionWindow()
        {
            InitializeComponent();
            Data.ViewModelLocator.Locator.FilesSelection.SelectedFiles = null;
        }

        /// <summary>
        /// Sets the FilesSelectionViewModel.SelectedFiles
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Data.FilesSelectionViewModel context = this.DataContext as Data.FilesSelectionViewModel;
            context.SelectedFiles = dgvFiles.SelectedItems.Cast<System.IO.FileInfo>();
        }

        /// <summary>
        /// Select a base folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var filesSelection = Data.ViewModelLocator.Locator.FilesSelection;
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            // Default path
            if (new DirectoryInfo(filesSelection.FolderBase).Exists)
            {
                dialog.SelectedPath = filesSelection.FolderBase;
            }

            // Open dialogbox
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filesSelection.FolderBase = dialog.SelectedPath;
            }
        }
    }
}
