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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of Login Window
        /// </summary>
        public LoginWindow() : this(null, null)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of Login Window
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="credentials"></param>
        public LoginWindow(Window owner, ServerCredentials credentials)
        {
            InitializeComponent();

            this.Owner = owner;
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

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
            this.Credentials.SaveToRegistry();
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
        /// Initializes a new instance of ServerCredentials and set default values
        /// </summary>
        public ServerCredentials()
        {
            // Gets all Login configurations from local registry
            this.LoadFromRegistry();
        }

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

        /// <summary>
        /// Load all property values from the local registry
        /// </summary>
        public void LoadFromRegistry()
        {
            using (Registry registry = new Registry(ReportServices.REGISTRY_APPLICATIONNAME, "Credentials"))
            {
                this.ServerUrl = registry.GetValue("ServerUrl");
                this.Login = registry.GetValue("Login");
                this.Password = registry.GetValue("Password");
                this.Domain = registry.GetValue("Domain");
            }

            // Sets default values
            if (String.IsNullOrEmpty(this.ServerUrl))
            {
                this.ServerUrl = "http://localhost/ReportServer";
            }

            if (String.IsNullOrEmpty(this.Login))
            {
                this.Login = "ReportUser";
            }

            if (String.IsNullOrEmpty(this.Password))
            {
                this.Password = "Password";
            }

            if (String.IsNullOrEmpty(this.Domain))
            {
                this.Domain = String.Empty;
            }
        }

        /// <summary>
        /// Save all property values into the local registry
        /// </summary>
        public void SaveToRegistry()
        {
            using (Registry registry = new Registry(ReportServices.REGISTRY_APPLICATIONNAME, "Credentials"))
            {
                registry.SetValue("ServerUrl", this.ServerUrl);
                registry.SetValue("Login", this.Login);
                registry.SetValue("Password", this.Password);
                registry.SetValue("Domain", this.Domain);
            }
        }
    }
}
