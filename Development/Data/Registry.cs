using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportTransfert.Data
{
    /// <summary>
    /// Class to load and save configuration data in the user registry
    /// at HKEY_CURRENT_USER\Software\Voituron\ or defined by the Root property
    /// </summary>
    public class Registry : IDisposable
    {
        #region DECLARATION

        private const string _defaultRoot = @"Software\Voituron\";

        private Microsoft.Win32.RegistryKey _currentSubKey;
        private string _root = _defaultRoot;
        private string _application = String.Empty;
        private string _section = String.Empty;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Initialize a new instance of registry
        /// </summary>
        public Registry()
            : this(_defaultRoot, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initialize a new instance of registry
        /// </summary>
        /// <param name="application">Software name</param>
        /// <param name="section">Configuration section name</param>
        public Registry(string application, string section)
            : this(_defaultRoot, application, section)
        {
        }

        /// <summary>
        /// Initialize a new instance of registry
        /// </summary>
        /// <param name="root">Root name (ex. Software\Voituron)</param>
        /// <param name="application">Software name</param>
        /// <param name="section">Configuration section name</param>
        public Registry(string root, string application, string section)
        {
            this.Root = root;
            this.Application = application;
            this.Section = section;
        }

        /// <summary>
        /// Initialize a new instance of registry
        /// </summary>
        /// <param name="path">Full path to the registry folder (ex. Software\Voituron\MyApplication\MyData)</param>
        public Registry(string path)
        {
            string[] parts = path.Split('\\');

            if (parts.Length < 3)
                throw new ArgumentException("You must have at least a root item, an application item and a section item (ex. Software\\MyApplication\\MyData).");

            this.Root = String.Join("\\", parts, 0, parts.Length - 2);
            this.Application = parts[parts.Length - 2];
            this.Section = parts[parts.Length - 1];
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the root folder
        /// </summary>
        public string Root
        {
            set
            {
                if (value.EndsWith("\\"))
                    _root = value;
                else
                    _root = value + "\\";

                ChangeCurrentSubKey();

            }
            get
            {
                return _root;
            }
        }

        /// <summary>
        /// Gets or sets the software name
        /// </summary>
        public string Application
        {
            set
            {
                if (value.EndsWith("\\"))
                    _application = value;
                else
                    _application = value + "\\";

                ChangeCurrentSubKey();
            }
            get
            {
                return _application;
            }
        }

        /// <summary>
        /// Gets or sets the section configuration name
        /// </summary>
        public string Section
        {
            set
            {
                if (value.EndsWith("\\"))
                    _section = value;
                else
                    _section = value + "\\";

                ChangeCurrentSubKey();
            }
            get
            {
                return _section;
            }
        }

        /// <summary>
        /// Get the count of items included in this section
        /// </summary>
        public int KeysCount
        {
            get
            {
                if (_currentSubKey != null)
                    return _currentSubKey.ValueCount;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets all keys included in this section
        /// </summary>
        public string[] Keys
        {
            get
            {
                if (_currentSubKey != null)
                    return _currentSubKey.GetValueNames();
                else
                    return new string[] { };
            }
        }

        /// <summary>
        /// Gets the count of sub sections included in this section
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
        public int SubKeysCount
        {
            get
            {
                if (_currentSubKey != null)
                    return _currentSubKey.SubKeyCount;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets the a list of sub sections included in this section
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
        public string[] SubKeys
        {
            get
            {
                if (_currentSubKey != null)
                    return _currentSubKey.GetSubKeyNames();
                else
                    return new string[] { };
            }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Gets the key value associated to Root, Application and Section
        /// </summary>
        /// <param name="key">Nom de la clé</param>
        /// <returns>Valeur de la clé</returns>
        public string GetValue(string key)
        {
            if (_currentSubKey != null)
                return _currentSubKey.GetValue(key, String.Empty).ToString();
            else
                return string.Empty;
        }

        /// <summary>
        /// Sets or create the value associated to Root, Application and Section
        /// </summary>
        /// <param name="key">Key name</param>
        /// <param name="value">Value</param>
        public void SetValue(string key, string value)
        {
            Microsoft.Win32.RegistryKey reg = Microsoft.Win32.Registry.CurrentUser;
            reg = reg.CreateSubKey(_root + _application + _section);
            reg.SetValue(key, value);
        }

        /// <summary>
        /// Delete the key associated to Root, Application and Section
        /// </summary>
        /// <param name="key">Key name</param>
        public void DeleteValue(string key)
        {
            Microsoft.Win32.RegistryKey reg = Microsoft.Win32.Registry.CurrentUser;
            reg = reg.CreateSubKey(_root + _application + _section);
            reg.DeleteSubKey(key);
        }

        /// <summary>
        /// Delete the current section (Root, Application and Section)
        /// </summary>
        public void DeleteSection()
        {
            Microsoft.Win32.RegistryKey reg = Microsoft.Win32.Registry.CurrentUser;
            reg.DeleteSubKeyTree(_root + _application + _section);
        }

        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose all resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects).                
            }

            // Free your own state (unmanaged objects).
            // Set large fields to null.
            if (_currentSubKey != null)
            {
                _currentSubKey.Close();
                _currentSubKey.Dispose();
            }
        }

        /// <summary>
        /// finalization code.
        /// </summary>
        ~Registry()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        #endregion

        #region PRIVATES

        /// <summary>
        /// Change the current key to Root / Application / Section
        /// </summary>
        private void ChangeCurrentSubKey()
        {
            _currentSubKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(_root + _application + _section);
        }

        #endregion

    }
}
