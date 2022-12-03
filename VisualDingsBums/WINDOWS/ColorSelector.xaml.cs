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
    /// Interaktionslogik für ColorSelector.xaml
    /// </summary>
    public partial class ColorSelector : Window, INotifyPropertyChanged
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
        #region [ IntColor ]
        /// <summary>
        /// Color as integer.
        /// </summary>
        public int IntColor
        {
            get
            {
                byte[] colorBytes = new byte[4];
                colorBytes[0] = this.A;
                colorBytes[1] = this.R;
                colorBytes[2] = this.G;
                colorBytes[3] = this.B;
                return BitConverter.ToInt32(colorBytes, 0);
            }
        }
        #endregion

        #region [ TextColor ]
        private string _textColor;

        /// <summary>
        /// Color as text.
        /// </summary>
        public string TextColor
        {
            get
            {
                return this._textColor;
            }

            set
            {
                this._textColor = value;
                OnPropertyChanged("TextColor");
            }
        }
        #endregion

        #region [ BrushColor ]
        private Brush _brushColor;

        /// <summary>
        /// Brush value of the color.
        /// </summary>
        public Brush BrushColor
        {
            get
            {
                return this._brushColor;
            }

            set
            {
                this._brushColor = value;
                OnPropertyChanged("BrushColor");
            }
        }
        #endregion

        #region [ A Channel ]
        private byte _a;

        /// <summary>
        /// A Channel
        /// </summary>
        public byte A
        {
            get
            {
                return this._a;
            }

            set
            {
                this._a = value;
                OnPropertyChanged("A");

                Color c = Color.FromArgb(this.A, this.R, this.G, this.B);
                this.BrushColor = new SolidColorBrush(c);
                this.TextColor = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
            }
        } 
        #endregion

        #region [ R Channel ]
        private byte _r;

        /// <summary>
        /// R Channel
        /// </summary>
        public byte R
        {
            get
            {
                return this._r;
            }

            set
            {
                this._r = value;
                OnPropertyChanged("R");

                Color c = Color.FromArgb(this.A, this.R, this.G, this.B);
                this.BrushColor = new SolidColorBrush(c);
                this.TextColor = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
            }
        }
        #endregion

        #region [ G Channel ]
        private byte _g;

        /// <summary>
        /// G Channel
        /// </summary>
        public byte G
        {
            get
            {
                return this._g;
            }

            set
            {
                this._g = value;
                OnPropertyChanged("G");

                Color c = Color.FromArgb(this.A, this.R, this.G, this.B);
                this.BrushColor = new SolidColorBrush(c);
                this.TextColor = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
            }
        }
        #endregion

        #region [ B Channel ]
        private byte _b;

        /// <summary>
        /// B Channel
        /// </summary>
        public byte B
        {
            get
            {
                return this._b;
            }

            set
            {
                this._b = value;
                OnPropertyChanged("B");

                Color c = Color.FromArgb(this.A, this.R, this.G, this.B);
                this.BrushColor = new SolidColorBrush(c);
                this.TextColor = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
            }
        }
        #endregion
        #endregion

        #region [ Constructor ]
        public ColorSelector()
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
            this.DialogResult = true;
        }
        #endregion
    }
}
