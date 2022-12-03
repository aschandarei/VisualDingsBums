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
    /// Interaktionslogik für DialectBrowser.xaml
    /// </summary>
    public partial class DialectBrowser : Window, INotifyPropertyChanged
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
        #region [ Mainindex ]
        private int _mainindex;

        /// <summary>
        /// Selection index of the Main-listview.
        /// </summary>
        public int Mainindex
        {
            get
            {
                return this._mainindex;
            }

            set
            {
                this._mainindex = value;
                OnPropertyChanged("Mainindex");
            }

        }
        #endregion
        
        #region [ Subindex ]
        private int _subindex;

        /// <summary>
        /// Selection index of the sub-listview.
        /// </summary>
        public int Subindex
        {
            get
            {
                return this._subindex;
            }

            set
            {
                this._subindex = value;
                OnPropertyChanged("Subindex");
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
        public DialectBrowser()
        {
            InitializeComponent();
        } 
        #endregion

        #region [ Events ]
        /// <summary>
        /// Add new dialect with standart BF contents (but name and extension).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuAddNewDialect_Click(object sender, RoutedEventArgs e)
        {
            InputBox ib = new InputBox();
            ib.Hint = "Input new dialect name:";
            ib.Text = "NewBFdialect";
            ib.Owner = this;
            if ((bool)ib.ShowDialog())
            {
                string name = ib.Text;
                ib = new InputBox();
                ib.Hint = "Input new file extension:";
                ib.Text = ".BFdialect";
                if ((bool)ib.ShowDialog())
                {
                    BrainFuckDialect bfd = new BrainFuckDialect();
                    bfd.ChangeName(name);
                    bfd.ChangeExtension(ib.Text);

                    this.Dialects.Add(bfd);
                    OnPropertyChanged("Dialects");
                }
            }
        }

        /// <summary>
        /// Edit respective item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuEditItem_Click(object sender, RoutedEventArgs e)
        {
            string Tag = ((MenuItem)sender).Tag.ToString();
            if (Tag.Equals("Name"))
            {
                InputBox ib = new InputBox();
                ib.Hint = "Input new dialect name:";
                ib.Text = this.Dialects[Mainindex].Name;
                ib.Owner = this;
                if ((bool)ib.ShowDialog())
                {
                    this.Dialects[Mainindex].ChangeName(ib.Text);
                    OnPropertyChanged("Dialects");
                }
            }
            else if (Tag.Equals("Extension"))
            {
                InputBox ib = new InputBox();
                ib.Hint = "Input new file extension:";
                ib.Text = this.Dialects[Mainindex].Extension;
                ib.Owner = this;
                if ((bool)ib.ShowDialog())
                {
                    this.Dialects[Mainindex].ChangeExtension(ib.Text);
                    OnPropertyChanged("Dialects");
                }
            }
            else if (Tag.Equals("Command"))
            {
                InputBox ib = new InputBox();
                ib.Hint = "Input new command text:";
                BrainFuckLanguageItem bfli = this.Dialects[Mainindex].DialectItems[Subindex];
                ib.Text = bfli.DialectText;
                ib.Owner = this;
                if ((bool)ib.ShowDialog())
                {
                    this.Dialects[Mainindex].ChangeDialectText(bfli.Command, ib.Text);
                    OnPropertyChanged("Dialects");
                }
            }
            else if (Tag.Equals("Style"))
            {
                FontStyleSelector fs = new FontStyleSelector();
                BrainFuckLanguageItem bfli = this.Dialects[Mainindex].DialectItems[Subindex];
                fs.Owner = this;
                if ((bool)fs.ShowDialog())
                {
                    this.Dialects[Mainindex].ChangeFontStyle(bfli.Command, fs.SelectedStyle);
                    OnPropertyChanged("Dialects");
                }
            }
            else if (Tag.Equals("Weight"))
            {
                FontWeightSelector fs = new FontWeightSelector();
                BrainFuckLanguageItem bfli = this.Dialects[Mainindex].DialectItems[Subindex];
                fs.Owner = this;
                if ((bool)fs.ShowDialog())
                {
                    this.Dialects[Mainindex].ChangeFontWeight(bfli.Command, fs.SelectedWeight);
                    OnPropertyChanged("Dialects");
                }
            }
            else if (Tag.Equals("Color"))
            {
                ColorSelector cs = new ColorSelector();
                BrainFuckLanguageItem bfli = this.Dialects[Mainindex].DialectItems[Subindex];
                cs.Owner = this;
                Color c = ((SolidColorBrush)bfli.Foreground).Color;
                cs.A = c.A;
                cs.B = c.B;
                cs.R = c.R;
                cs.G = c.G;

                if ((bool)cs.ShowDialog())
                {
                    this.Dialects[Mainindex].ChangeColor(bfli.Command, cs.BrushColor);
                    OnPropertyChanged("Dialects");
                }
            }
        }

        /// <summary>
        /// Ensure scroll events are reached further to the main list view. (Thus ensure proper scrolling)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        /// <summary>
        /// Ensure click events are reached further to the main list view. (Thus ensuring proper main index.)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                var eventArg = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton);
                eventArg.RoutedEvent = UIElement.MouseDownEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

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
