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
    /// Interaktionslogik für BrainVixx.xaml
    /// </summary>
    public partial class BrainVixx : Window, INotifyPropertyChanged
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
        #region [ Runs ]
        private ObservableCollection<Run> _runs;

        public ObservableCollection<Run> Runs
        {
            get
            {
                return this._runs;
            }

            set
            {
                this._runs = value;
                OnPropertyChanged("Runs");
            }
        }
        #endregion
        #endregion

        #region [ Fields ]
        WindowMemoryView wmv;
        BrainFuckDialect bfd;
        STDOUTView stdout;
        #endregion

        #region [ Constructor ]
        public BrainVixx(WindowMemoryView wmv, STDOUTView stdout, BrainFuckDialect bfd)
        {
            InitializeComponent();
            this.wmv = wmv;
            this.bfd = bfd;
            this.stdout = stdout;
            this.Runs = new ObservableCollection<Run>();
        } 
        #endregion

        #region [ Events ]
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Move_to_previous_Click(object sender, RoutedEventArgs e)
        {
            if (wmv.Previous())
            {
                Run newRun = new Run();
                BrainFuckLanguageItem bfli = this.bfd.DialectItems.First(di => di.Command == BrainFuckCommand.Move_to_previous_cell);
                newRun.Text = bfli.DialectText;
                newRun.FontStyle = bfli.FontStyle;
                newRun.Foreground = bfli.Foreground;
                newRun.Name = "Command";
                newRun.ToolTip = bfli.Command.ToString();
                this.Runs.Add(newRun);
            }
        }

        private void Move_to_next_Click(object sender, RoutedEventArgs e)
        {
            wmv.Next();
            Run newRun = new Run();
            BrainFuckLanguageItem bfli = this.bfd.DialectItems.First(di => di.Command == BrainFuckCommand.Move_to_next_cell);
            newRun.Text = bfli.DialectText;
            newRun.FontStyle = bfli.FontStyle;
            newRun.Foreground = bfli.Foreground;
            newRun.Name = "Command";
            newRun.ToolTip = bfli.Command.ToString();
            this.Runs.Add(newRun);
        }

        private void Increment_Click(object sender, RoutedEventArgs e)
        {
            wmv.Increment();
            Run newRun = new Run();
            BrainFuckLanguageItem bfli = this.bfd.DialectItems.First(di => di.Command == BrainFuckCommand.Increment_cell);
            newRun.Text = bfli.DialectText;
            newRun.FontStyle = bfli.FontStyle;
            newRun.Foreground = bfli.Foreground;
            newRun.Name = "Command";
            newRun.ToolTip = bfli.Command.ToString();
            this.Runs.Add(newRun);
        }

        private void Decrement_Click(object sender, RoutedEventArgs e)
        {
            wmv.Decrement();
            Run newRun = new Run();
            BrainFuckLanguageItem bfli = this.bfd.DialectItems.First(di => di.Command == BrainFuckCommand.Decrement_cell);
            newRun.Text = bfli.DialectText;
            newRun.FontStyle = bfli.FontStyle;
            newRun.Foreground = bfli.Foreground;
            newRun.Name = "Command";
            newRun.ToolTip = bfli.Command.ToString();
            this.Runs.Add(newRun);
        }

        private void Clear_memory_Click(object sender, RoutedEventArgs e)
        {
            wmv.InitializeMemory();
            this.Runs = new ObservableCollection<Run>();
        }

        private void Input_Click(object sender, RoutedEventArgs e)
        {
            STDINView stdin = new STDINView();
            stdin.Owner = this;
            stdin.ShowDialog();
            wmv.FromStdIN(stdin.Result);
            stdin.Close();
            Run newRun = new Run();
            BrainFuckLanguageItem bfli = this.bfd.DialectItems.First(di => di.Command == BrainFuckCommand.Input_cell);
            newRun.Text = bfli.DialectText;
            newRun.FontStyle = bfli.FontStyle;
            newRun.Foreground = bfli.Foreground;
            newRun.Name = "Command";
            newRun.ToolTip = bfli.Command.ToString();
            this.Runs.Add(newRun);
        }

        private void Output_Click(object sender, RoutedEventArgs e)
        {
            BrainFuckLanguageItem bfli = this.bfd.DialectItems.First(di => di.Command == BrainFuckCommand.Output_cell);
            Run newRun = new Run();
            newRun.Text = bfli.DialectText;
            newRun.FontStyle = bfli.FontStyle;
            newRun.Foreground = bfli.Foreground;
            newRun.Name = "Command";
            newRun.ToolTip = bfli.Command.ToString();
            this.Runs.Add(newRun);
            stdout.STDOUT += wmv.ToStdOUT();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Clear_last_Click(object sender, RoutedEventArgs e)
        {
            if (this.Runs.Count > 0)
            {
                this.Runs.RemoveAt(this.Runs.Count - 1);
            }
        } 
        #endregion
    }
}
