using ReportTransfert.ReportService2010;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReportTransfert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        /// <summary>
        /// Initializes a new instance of Main Window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            this.Title = String.Format("SQL Server Reporting Services - Transfert tools - v.{0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        /// <summary>
        /// Sets the MainViewModel.SelectedReports
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Data.MainViewModel context = this.DataContext as Data.MainViewModel;
            context.SelectedReports = dgvReports.SelectedItems.Cast<Data.Report>();
        }
    }
}
