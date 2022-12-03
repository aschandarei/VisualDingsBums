using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaktionslogik für DialectSelector.xaml
    /// </summary>
    public partial class DialectSelector : Window
    {
        #region [ Properties ]
        #region [ Selected dialect ]
        public BrainFuckDialect Dialect
        {
            get
            {
                return (BrainFuckDialect)this.ComboBoxSelection.SelectedItem;
            }

        } 
        #endregion

        #region [ Dialects ]
        public ObservableCollection<BrainFuckDialect> Dialects
        {
            get
            {
                return BFDialects.Dialects;
            }
        } 
        #endregion
        #endregion

        #region [ Constructor ]
        public DialectSelector()
        {
            InitializeComponent();
        } 
        #endregion

        #region [ Events ]
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (this.ComboBoxSelection.SelectedItem != null)
            {
                this.DialogResult = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialectBrowser db = new DialectBrowser();
            db.Owner = this;
            db.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.ComboBoxSelection.SelectedItem == null)
            {
                this.DialogResult = false;
            }
        }
        #endregion
    }
}
