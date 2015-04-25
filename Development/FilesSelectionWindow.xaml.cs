using System;
using System.Collections.Generic;
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
    }
}
