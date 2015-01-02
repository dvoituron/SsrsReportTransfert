using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window
    {
        public DownloadWindow()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Result = new DownloadParameters() 
            { 
                TargetFolder = txtTargetFolder.Text,
                ReplaceSource = txtSource.Text,
                ReplaceTarget = txtTarget.Text,
            };
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = new DownloadParameters();
            this.DialogResult = false;
            this.Close();
        }

        public DownloadParameters Result { get; set; }
    }

    public class DownloadParameters
    {
        public string TargetFolder { get; set; }
        public string ReplaceSource { get; set; }
        public string ReplaceTarget { get; set; }
    }
}
