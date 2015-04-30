using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace ReportTransfert.Data
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ServerCredentials _credentials = new ServerCredentials();
        private ReportServices _service = null;
        private bool _isWaiting;
        private IEnumerable<Report> _selectedReports = null;
        private double _progressPercent = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        #region CONSTRUCTORS

        /// <summary>
        /// Initializes a new instance of Main ViewModel
        /// </summary>
        public MainViewModel()
        {
            this.Reports = new ObservableCollection<Report>();

            this.OpenCommand = new RelayCommand(async (p) => await this.OpenExecute(p));
            this.RefreshCommand = new RelayCommand(async (p) => await this.RefreshExecute(), (p) => !String.IsNullOrEmpty(this.ServerUrl));
            this.DownloadCommand = new RelayCommand(async (p) => await this.DownloadExecute(p), (p) => _selectedReports != null && _selectedReports.Count() > 0);
            this.UploadCommand = new RelayCommand(async (p) => await this.UploadExecute(p), (p) => _selectedReports != null && _selectedReports.Count() == 1 && _selectedReports.First().IsFolder);
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets the command to execute when the user click on Open button
        /// </summary>
        public RelayCommand OpenCommand { get; private set; }

        /// <summary>
        /// Gets the command to execute when the user click on Refresh button
        /// </summary>
        public RelayCommand RefreshCommand { get; private set; }

        /// <summary>
        /// Gets the command to execute when the user click on Download button
        /// </summary>
        public RelayCommand DownloadCommand { get; private set; }

        /// <summary>
        /// Gets the command to execute when the user click on Upload button
        /// </summary>
        public RelayCommand UploadCommand { get; private set; }

        /// <summary>
        /// Gets a list with all reports items (folders, images, resources, reports)
        /// </summary>
        public ObservableCollection<Report> Reports { get; private set; }

        /// <summary>
        /// Gets or sets the selected reports
        /// </summary>
        public IEnumerable<Report> SelectedReports
        {
            get
            {
                return _selectedReports;
            }
            set
            {
                _selectedReports = value;
                OnPropertyChanged("SelectedReports");
            }
        }

        /// <summary>
        /// Gets the SSRS Server URL
        /// </summary>
        public string ServerUrl { get; private set; }

        /// <summary>
        /// Gets or sets True to display an Indeterminate progress bar
        /// </summary>
        public bool IsWaiting
        {
            get
            {
                return _isWaiting;
            }
            set
            {
                _isWaiting = value;
                this.OnPropertyChanged("IsWaiting");
                DoEvents();
            }
        }

        /// <summary>
        /// Gets or sets the percent value for ProgressBar
        /// </summary>
        public double ProgressPercent
        {
            get
            {
                return _progressPercent;
            }
            set
            {
                _progressPercent = value;
                this.OnPropertyChanged("ProgressPercent");
                DoEvents();
            }

        }
        
        #endregion

        #region COMMANDS

        /// <summary>
        /// Open the Login window and connect to the SSRS server
        /// </summary>
        /// <param name="parameter"></param>
        private async Task OpenExecute(object parameter)
        {
            LoginWindow frmLogin = new LoginWindow(parameter as Window, _credentials);
            if (frmLogin.ShowDialog() == true)
            {
                _credentials = frmLogin.Credentials;
                _service = new ReportServices(_credentials.ServerUrl, _credentials.Login, _credentials.Password, _credentials.Domain);
                this.ServerUrl = _credentials.ServerUrl;
                this.OnPropertyChanged("ServerUrl");

                await this.RefreshExecute();
            }
        }

        /// <summary>
        /// Reload all reports from the SSRS server
        /// </summary>
        private async Task RefreshExecute()
        {
            this.IsWaiting = true;
            this.Reports = new ObservableCollection<Data.Report>();

            if (_service != null)
            {
                try
                {
                    ReportTransfert.ReportService2010.CatalogItem[] items = await _service.GetItems("/");
                    this.Reports = new ObservableCollection<Data.Report>(items.Select(i => new Data.Report(_service, i)));
                }
                catch (Exception ex)
                {
                    this.DisplayException(ex);
                }
            }
            this.OnPropertyChanged("Reports");
            this.IsWaiting = false;
        }

        /// <summary>
        /// Ask a directory to download all selected reports
        /// </summary>
        private async Task DownloadExecute(object parameter)
        {
            int reportsCount = this.SelectedReports.Count();
            if (reportsCount > 0)
            {

                DownloadWindow frmDownload = new DownloadWindow();
                frmDownload.Owner = parameter as Window;
                frmDownload.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                if (frmDownload.ShowDialog() == true)
                {
                    try
                    {
                        // Save all reports
                        int n = 0;
                        foreach (Data.Report report in this.SelectedReports)
                        {
                            // Progress
                            n++;
                            this.ProgressPercent = Convert.ToDouble(n) / Convert.ToDouble(reportsCount) * 100d;
                            DoEvents();


                            // Download
                            string filename = await report.DownloadToFolder(frmDownload.Result.TargetFolder);

                            // Replace old string by new string
                            if (!String.IsNullOrEmpty(filename) && !String.IsNullOrEmpty(frmDownload.Result.ReplaceSource))
                            {
                                string content = System.IO.File.ReadAllText(filename);
                                string newContent = Regex.Replace(
                                    content,
                                    Regex.Escape(frmDownload.Result.ReplaceSource),
                                    frmDownload.Result.ReplaceTarget,
                                    RegexOptions.IgnoreCase);

                                System.IO.File.WriteAllText(filename, newContent);
                            }

                        }

                        MessageBox.Show("Download completed.", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        this.DisplayException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Uploads files to the selected Remote folder.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private async Task UploadExecute(object parameters)
        {
            Report remoteFolder = this.SelectedReports.FirstOrDefault();

            if (remoteFolder != null && remoteFolder.IsFolder)
            {
                FilesSelectionWindow selection = new FilesSelectionWindow();
                if (selection.ShowDialog() == true && ViewModelLocator.Locator.FilesSelection.SelectedFiles.Count() > 0)
                {
                    IEnumerable<FilesSelectionItem> files = ViewModelLocator.Locator.FilesSelection.SelectedFiles;
                    DirectoryInfo folderBase = new DirectoryInfo(ViewModelLocator.Locator.FilesSelection.FolderBase);

                    try
                    {
                        int filesCount = files.Count();
                        int n = 0;
                        foreach (FilesSelectionItem file in files)
                        {
                            // Progress
                            n++;
                            this.ProgressPercent = Convert.ToDouble(n) / Convert.ToDouble(filesCount) * 100d;
                            DoEvents();

                            // Upload
                            await remoteFolder.UploadFileInThisFolder(file.File, folderBase);
                        }

                        MessageBox.Show("Upload completed.", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        this.DisplayException(ex);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please, select a folder to upload files.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region PRIVATES

        /// <summary>
        /// Release the UI process
        /// </summary>
        private void DoEvents()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        /// <summary>
        /// Raises the PropertyChanged to notify UI
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Display a system error.
        /// </summary>
        /// <param name="ex"></param>
        private void DisplayException(Exception ex)
        {
            string message = ex.Message;
            if (ex.InnerException != null)
            {
                message += Environment.NewLine + ex.InnerException.Message;
            }
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}
