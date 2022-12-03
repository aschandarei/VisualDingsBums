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
    /// Interaktionslogik für FontStyleSelector.xaml
    /// </summary>
    public partial class FontWeightSelector : Window
    {
        #region [ Properties ]
        /// <summary>
        /// Selected font style. (Normal is default).
        /// </summary>
        public FontWeight SelectedWeight
        {
            get
            {
                if (this.ComboBoxFontStyle.SelectedItem == null)
                {
                    return FontWeights.Normal;
                }
                else
                {
                    string item = this.ComboBoxFontStyle.Text;

                    if (item.Equals("Bold"))
                    {
                        return FontWeights.Bold;
                    }
                    else if (item.Equals("Medium"))
                    {
                        return FontWeights.Medium;
                    }

                    return FontWeights.Normal;
                }
            }
        }
        #endregion

        #region [ Constructor ]
        public FontWeightSelector()
        {
            InitializeComponent();
        }
        #endregion

        #region [ Events ]
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.ComboBoxFontStyle.SelectedItem == null)
            {
                return;
            }

            this.DialogResult = true;
        } 
        #endregion
    }
}
