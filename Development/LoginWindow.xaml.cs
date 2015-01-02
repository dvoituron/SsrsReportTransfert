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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of Login Window
        /// </summary>
        public LoginWindow() : this(null)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of Login Window
        /// </summary>
        /// <param name="credentials"></param>
        public LoginWindow(ServerCredentials credentials)
        {
            InitializeComponent();

            if (credentials != null)
            {
                txtServerUrl.Text = credentials.ServerUrl;
                txtLogin.Text = credentials.Login;
                txtPassword.Password = credentials.Password;
                txtDomain.Text = credentials.Domain;
            }
        }

        /// <summary>
        /// Gets the server configuration set by the user.
        /// </summary>
        public ServerCredentials Credentials { get; private set; }

        /// <summary>
        /// Close this login window and returns items in Credentials property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Credentials = new ServerCredentials()
            {
                ServerUrl = txtServerUrl.Text,
                Login = txtLogin.Text,
                Password = txtPassword.Password,
                Domain = txtDomain.Text
            };
            this.Close();
        }

        /// <summary>
        /// Close this login window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Credentials = null;
            this.DialogResult = false;
            this.Close();
        }
    }

    /// <summary>
    /// Server configurations
    /// </summary>
    public class ServerCredentials
    {
        /// <summary>
        /// Gets or sets the Server URL (http://xxx:80/ReportServer)
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// Gets or sets the login to connect to ServerUrl
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets the password to connect to ServerUrl
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the domain to connect to ServerUrl
        /// </summary>
        public string Domain { get; set; }
    }
}
