using ReportTransfert.Data;
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
        /// <summary>
        /// Initializes a new instance of Download window.
        /// </summary>
        public DownloadWindow()
        {
            InitializeComponent();

            DownloadParameters parameters = new DownloadParameters();
            txtTargetFolder.Text = parameters.TargetFolder;
            txtSource.Text = parameters.ReplaceSource;
            txtTarget.Text = parameters.ReplaceTarget;
        }

        /// <summary>
        /// Close this Download window and returns items 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Result = new DownloadParameters() 
            { 
                TargetFolder = txtTargetFolder.Text,
                ReplaceSource = txtSource.Text,
                ReplaceTarget = txtTarget.Text,
            };
            this.Result.SaveToRegistry();
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Close this window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = new DownloadParameters();
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Properties selected
        /// </summary>
        public DownloadParameters Result { get; set; }
    }

    /// <summary>
    /// Items wrote by user
    /// </summary>
    public class DownloadParameters
    {
        /// <summary>
        /// Initializes a new instance of DownloadParameters
        /// </summary>
        public DownloadParameters()
        {
            this.LoadFromRegistry();
        }

        /// <summary>
        /// Gets or sets the local target folder
        /// </summary>
        public string TargetFolder { get; set; }

        /// <summary>
        /// Gets or sets the string to be replace
        /// </summary>
        public string ReplaceSource { get; set; }

        /// <summary>
        /// Gets or sets the replacement string
        /// </summary>
        public string ReplaceTarget { get; set; }

        /// <summary>
        /// Load all property values from the local registry
        /// </summary>
        public void LoadFromRegistry()
        {
            using (Registry registry = new Registry(ReportServices.REGISTRY_APPLICATIONNAME, "Download"))
            {
                this.TargetFolder = registry.GetValue("TargetFolder");
                this.ReplaceSource = registry.GetValue("ReplaceSource");
                this.ReplaceTarget = registry.GetValue("ReplaceTarget");
            }

            // Sets default values
            if (String.IsNullOrEmpty(this.TargetFolder))
            {
                this.TargetFolder = "C:\\Temp";
            }
            
        }

        /// <summary>
        /// Save all property values into the local registry
        /// </summary>
        public void SaveToRegistry()
        {
            using (Registry registry = new Registry(ReportServices.REGISTRY_APPLICATIONNAME, "Download"))
            {
                registry.SetValue("TargetFolder", this.TargetFolder);
                registry.SetValue("ReplaceSource", this.ReplaceSource);
                registry.SetValue("ReplaceTarget", this.ReplaceTarget);
            }
        }
    }
}
