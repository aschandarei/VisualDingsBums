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
    public partial class FontStyleSelector : Window
    {
        #region [ Properties ]
        /// <summary>
        /// Selected font style. (Normal is default).
        /// </summary>
        public FontStyle SelectedStyle
        {
            get
            {
                if (this.ComboBoxFontStyle.SelectedItem == null)
                {
                    return FontStyles.Normal;
                }
                else
                {
                    string item = this.ComboBoxFontStyle.Text;

                    if (item.Equals("Italic"))
                    {
                        return FontStyles.Italic;
                    }
                    else if (item.Equals("Oblique"))
                    {
                        return FontStyles.Oblique;
                    }

                    return FontStyles.Normal;
                }
            }
        }
        #endregion

        #region [ Constructor ]
        public FontStyleSelector()
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
