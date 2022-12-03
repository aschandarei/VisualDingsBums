using System;
using System.Collections.Generic;
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
    /// Interaktionslogik für STDINView.xaml
    /// </summary>
    public partial class STDINView : Window
    {
        #region [ Properties ]
        /// <summary>
        /// Char input from the STDIN.
        /// </summary>
        public char Result
        {
            get
            {
                if (this.TextBoxSTDIN.Text.Length>0)
                {
                    return this.TextBoxSTDIN.Text[0];
                }

                return (char)0;
            }
        }
        #endregion

        #region [ Constructor ]
        public STDINView()
        {
            InitializeComponent();
        } 
        #endregion

        #region [ Events ]
        private void TextBoxSTDIN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.DialogResult = true;
            }
        } 

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this.TextBoxSTDIN);
        }
        #endregion
    }
}
