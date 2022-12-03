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
using VisualDingsBums.VDBOBJECTS;

namespace VisualDingsBums.WINDOWS
{
    /// <summary>
    /// Interaktionslogik für RunStep.xaml
    /// </summary>
    public partial class RunStep : Window, INotifyPropertyChanged
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
        /// Hint text for the GUI.
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
            }
        }
        #endregion

        #region [ Result ]
        /// <summary>
        /// Step mode as chosen in the GUI.
        /// </summary>
        public StepMode Result { get; set; }
        #endregion

        #endregion

        #region [ Constructor ]
        public RunStep(string Title)
        {
            InitializeComponent();
            this.Title = Title;
        } 
        #endregion

        #region [ Events ]
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            if (clickedButton.Tag.Equals("Run"))
            {
                this.Result = StepMode.Run;
            }
            else if (clickedButton.Tag.Equals("NextStep"))
            {
                this.Result = StepMode.NextStep;
            }
            else
            {
                this.Result = StepMode.Cancel;
            }

            this.DialogResult = true;
        } 
        #endregion
    }
}
