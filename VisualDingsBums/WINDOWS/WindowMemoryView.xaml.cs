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
    /// Interaktionslogik für WindowMemoryView.xaml
    /// </summary>
    public partial class WindowMemoryView : Window, INotifyPropertyChanged
    {
        #region [ Fields ]
        private bool killCommand = false;
        #endregion

        #region [ INotifyPropertyChanged ]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region [ Properties ]
        #region [ Programm memory ]
        private ObservableCollection<MemoryItem> _programmMemory;

        /// <summary>
        /// Brain f*ck memory.
        /// </summary>
        public ObservableCollection<MemoryItem> ProgrammMemory
        {
            get
            {
                return this._programmMemory;
            }

            set
            {
                this._programmMemory = value;
                OnPropertyChanged("ProgrammMemory");
            }
        }
        #endregion

        #region [ Current memory item ]
        public MemoryItem _currentItem;

        /// <summary>
        /// Current selected memory item.
        /// </summary>
        public MemoryItem CurrentItem
        {
            get
            {
                return this._currentItem;
            }

            set
            {
                this._currentItem = value;
                OnPropertyChanged("CurrentItem");
            }
        }
        #endregion

        #region [ ClickPreventer ]
        private Visibility _clickPreventer;

        /// <summary>
        /// Prevents clicks on the list view at runtime.
        /// </summary>
        public Visibility ClickPreventer
        {
            get
            {
                return this._clickPreventer;
            }

            set
            {
                this._clickPreventer = value;
                OnPropertyChanged("ClickPreventer");
            }
        }
        #endregion
        #endregion

        #region [ Methods ]
        /// <summary>
        /// Kill the brain view window.
        /// </summary>
        public void Kill()
        {
            this.killCommand = true;
            this.Close();
        }

        /// <summary>
        /// Increments the value of the current memory cell.
        /// </summary>
        public void Increment()
        {
            CurrentItem.Value++;
        }

        /// <summary>
        /// Decrements the value of the current memory cell.
        /// </summary>
        public void Decrement()
        {
            CurrentItem.Value--;
        }

        /// <summary>
        /// Moves to the previous memory cell.
        /// </summary>
        public bool Previous()
        {
            int ID = CurrentItem.ID;
            ID--;

            if (ID >= 0)
            {
                CurrentItem = this.ProgrammMemory.First(pm => pm.ID == ID);
                return true;
            }
            else
            {
                MessageBox.Show("Out of memory!", "Visual DingsBums", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Moves to the next memory cell.
        /// </summary>
        public void Next()
        {
            int ID = CurrentItem.ID;
            ID++;

            if (ID < this.ProgrammMemory.Count)
            {
                CurrentItem = this.ProgrammMemory.First(pm => pm.ID == ID);
            }
            else
            {
                this.ProgrammMemory.Add(new MemoryItem(ID));
            }
            CurrentItem = this.ProgrammMemory.First(pm => pm.ID == ID);
            this.ListViewMemory.ScrollIntoView(CurrentItem);
        }

        /// <summary>
        /// Presents the current memory cell contents as char for the STDOUT.
        /// </summary>
        /// <returns>Char current memory cell content.</returns>
        public char ToStdOUT()
        {
            return CurrentItem.Char;
        }

        /// <summary>
        /// Writes char from the stdin into the current memory cell.
        /// </summary>
        /// <param name="Input"></param>
        public void FromStdIN(char Input)
        {
            CurrentItem.Value = (byte)Input;
        }

        /// <summary>
        /// Initializes the memory.
        /// </summary>
        public void InitializeMemory()
        {
            this._programmMemory = new ObservableCollection<MemoryItem>();
            for (int i = 0; i < 100; i++)
            {
                this._programmMemory.Add(new MemoryItem(i));
            }
            this.CurrentItem = this.ProgrammMemory[0];
            OnPropertyChanged("ProgrammMemory");
        }
        #endregion

        #region [ Constructor ]
        public WindowMemoryView()
        {
            InitializeComponent();
            InitializeMemory();
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
