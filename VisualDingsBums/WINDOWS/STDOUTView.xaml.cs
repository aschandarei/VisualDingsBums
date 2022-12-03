using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace VisualDingsBums
{
    /// <summary>
    /// Interaktionslogik für STDOUTView.xaml
    /// </summary>
    public partial class STDOUTView : Window, INotifyPropertyChanged
    {
        #region [ INotifyPropertyChanged ]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        } 
        #endregion

        #region [ Fields ]
        private bool killCommand = false;
        #endregion

        #region [ Properties ]
        #region [ STDOUT ]
        private string _stdout;

        /// <summary>
        /// Content of the stdout.
        /// </summary>
        public string STDOUT
        {
            get
            {
                return this._stdout;
            }

            set
            {
                this._stdout = value;
                OnPropertyChanged("STDOUT");
            }
        }
        #endregion

        #endregion

        #region [ Constructor ]
        public STDOUTView()
        {
            InitializeComponent();
        } 
        #endregion

        #region [ Methods ]
        /// <summary>
        /// Kill the stdout view window.
        /// </summary>
        public void Kill()
        {
            this.killCommand = true;
            this.Close();
        }
        #endregion

        #region [ Events ]
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!this.killCommand)
            {
                e.Cancel = true;
            }
        }
        #endregion
    }
}
