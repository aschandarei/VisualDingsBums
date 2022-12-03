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
    /// Interaktionslogik für InputBox.xaml
    /// </summary>
    public partial class InputBox : Window, INotifyPropertyChanged
    {
        #region [ INotifyPropertyChanged ]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region [ Properties ]
        #region [ Hint ]
        private string _hint;

        /// <summary>
        /// Hint text for the input box.
        /// </summary>
        public string Hint
        {
            get
            {
                return this._hint;
            }

            set
            {
                this._hint = value;
                OnPropertyChanged("Hint");
            }
        }
        #endregion

        #region [ Text ]
        private string _text;

        /// <summary>
        /// Text value of the input box.
        /// </summary>
        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                this._text = value;
                OnPropertyChanged("Text");
            }
        }
        #endregion
        #endregion

        #region [ Constructor ]
        public InputBox()
        {
            InitializeComponent();

            this.Title = "VisualDingsBums";
        } 
        #endregion

        #region [ Events ]
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        } 
        #endregion
    }
}
