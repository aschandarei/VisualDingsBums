using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using System.Timers;
using System.Windows.Threading;
using VisualDingsBums.WINDOWS;

namespace VisualDingsBums
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
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
        #region [ Filename ]
        private string _filename;

        public string FileName
        {
            get
            {
                return this._filename;
            }

            set
            {
                this._filename = value;
                SetTitle();
            }
        } 
        #endregion

        #region [ BF dialect ]
        BrainFuckDialect _bfDialect;

        /// <summary>
        /// BF dialect currently in use.
        /// </summary>
        BrainFuckDialect BFDialect
        {
            get
            {
                return this._bfDialect;
            }

            set
            {
                this._bfDialect = value;
                OnPropertyChanged("BFDialect");
                SetTitle();
            }
        } 
        #endregion

        #region [ Changed ]
        bool _changed;

        /// <summary>
        /// Indicates if the document content was changed.
        /// </summary>
        public bool Changed
        {
            get
            {
                return this._changed;
            }

            set
            {
                this._changed = value;
                SetTitle();
            }
        } 
        #endregion

        /// <summary>
        /// Indicates that the current documents contains commands.
        /// </summary>
        public bool CanRun
        {
            get
            {
                if (this.HasDocument)
                {
                    sourceCode = new List<Run>();
                    foreach (Paragraph p in this.rtfBox.Document.Blocks)
                    {
                        foreach (Run r in p.Inlines)
                        {
                            if (r.Name.Equals("Command"))
                            {
                                sourceCode.Add(r);
                            }
                        }
                    }

                    if (sourceCode.Count < 1)
                    {
                        return false;
                    }

                    return true;
                }

                return false;
            }
        }
        #endregion

        #region [ Fields ]
        /// <summary>
        /// Runs code highlighthing.
        /// </summary>
        private DispatcherTimer beautifyTimer;

        /// <summary>
        /// Settings dataset.
        /// </summary>
        private DataSet settingsDS;

        /// <summary>
        /// List of flow document runs containing bf commands.
        /// </summary>
        private List<Run> sourceCode;

        #region [ Application windows ]
        /// <summary>
        /// Brain view window.
        /// </summary>
        WindowMemoryView wmv;

        /// <summary>
        /// STDOUT window.
        /// </summary>
        STDOUTView stdoutView; 
        #endregion

        #region [ Constants ]
        /// <summary>
        /// Settings file - file name.
        /// </summary>
        const string settingsFile = "Settings.vdb";

        /// <summary>
        /// Name of the settins data table - windows sizes.
        /// </summary>
        const string WindowsSizes = "WindowsSizes";

        /// <summary>
        /// Name of the settins data table - dialects.
        /// </summary>
        const string Dialects = "Dialects";

        /// <summary>
        /// Name of the application.
        /// </summary>
        const string AppName = "VisualDingsBums"; 
        #endregion

        #region [ Application condition ]
        /// <summary>
        /// Indicates if the application is running code highlighting.
        /// </summary>
        private bool Beutifying = false;

        /// <summary>
        /// Current document content is not changed since last beutifying.
        /// </summary>
        private bool Beutified = true;

        /// <summary>
        /// Indicates if there is active document in the RTF box.
        /// </summary>
        private bool HasDocument = false;

        int currentLineNumber;
        int currentColumnNumber;
        int currentOffset;

        /// <summary>
        /// Indicates that change events are permited.
        /// </summary>
        bool changeEventsPermited = true;
        #endregion
        #endregion

        #region [ Constructor ]
        public MainWindow()
        {
            InitializeComponent();
            beautifyTimer = new DispatcherTimer();
            beautifyTimer.Interval = new TimeSpan(0,0,5);
            beautifyTimer.Tick += beautifyTimer_Tick;
        }
        #endregion

        #region [ Events ]
        #region [ File commands ]
        #region [ New ]
        private void FileNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.rtfBox.Document = new FlowDocument();
            ChangeDialect();
            beautifyTimer.Start();
            this.HasDocument = true;
        }

        private void FileNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        #endregion

        #region [ Open ]
        private void FileOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CheckAndSave();

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            string filter = "";

            foreach (BrainFuckDialect bfd in BFDialects.Dialects)
            {
                filter += bfd.Name + " documents (" + bfd.Extension + ")|*" + bfd.Extension + "|";
            }
            dlg.Filter = filter.Substring(0, filter.Length - 1);
            dlg.DefaultExt = BFDialects.Dialects[0].Extension;
            dlg.Multiselect = false;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;


            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string[] parts = GetExtensionAndFileName(dlg.FileName);
                this.FileName = parts[0];
                SetDialectByExtension(parts[1]);

                FlowDocument fd = new FlowDocument();
                foreach (string line in File.ReadAllLines(dlg.FileName))
                {
                    Paragraph P = new Paragraph();
                    Run run = new Run();
                    run.Text = line;
                    P.Inlines.Add(run);
                    fd.Blocks.Add(P);
                }
                this.rtfBox.Document = fd;

                Beautify();
                this.Changed = false;
                this.HasDocument = true;
                beautifyTimer.Start();
            }
        }

        private void FileOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        } 
        #endregion

        #region [ Save ]
        private void FileSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveSource();
        }

        private void FileSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Changed;
        }  
        #endregion

        #region [ SaveAs ]
        private void FileSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveSourceAs();
        }

        private void FileSaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Changed;
        }
        #endregion

        #region [ Application quit ]
        private void AppQuit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void AppQuit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        } 
        #endregion
        #endregion

        #region [ Run commands ]
        #region [ Run BF Programm ]
        private void RunStep_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.RunProgram(true);
        }

        private void RunStep_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CanRun;
        }

        private void RunFull_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.RunProgram(false);
        }

        private void RunFull_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CanRun;
        } 
        #endregion

        #region [ Breakpoints ]
        private void ToggleBreakpoint_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ToggleBreakpoint();
        }

        private void ToggleBreakpoint_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.HasDocument;
        }

        private void ClearAllBreakpoints_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (Paragraph p in this.rtfBox.Document.Blocks)
            {
                foreach (Run r in p.Inlines)
                {
                    if (r.Name.Equals("Command"))
                    {
                        ProgrammElement pe = (ProgrammElement)r.Tag;
                        pe.Breakpoint = false;
                        r.Background = Brushes.Transparent;
                    }
                }
            }
        }

        private void ClearAllBreakpoints_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.HasDocument;
        } 
        #endregion

        private void mnuBrainVixx_Click(object sender, RoutedEventArgs e)
        {
            if (!this.rtfBox.IsEnabled)
            {
                this.rtfBox.Document = new FlowDocument();
                ChangeDialect();
                this.rtfBox.IsEnabled = true;
                this.HasDocument = true;
            }

            BrainVixx bv = new BrainVixx(this.wmv, this.stdoutView, this.BFDialect);
            bv.Owner = this;
            if ((bool)bv.ShowDialog())
            {
                Paragraph newParagraph = new Paragraph();
                foreach (Run r in bv.Runs)
                {
                    newParagraph.Inlines.Add(r);
                }
                this.rtfBox.Document.Blocks.Add(newParagraph);

                Beautify();
                beautifyTimer.Start();
                this.Changed = true;
            }
        }
        #endregion

        #region [ Edit commands ]
        private void ChangeDialect_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ChangeDialect();
        }

        private void ChangeDialect_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.HasDocument;
        }
        #endregion

        #region [ Rich text box ]
        private void rtfBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.changeEventsPermited)
            {
                this.Changed = true;
                this.Beutified = false;
            }
        }

        private void rtfBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            CurrentCaretPosition();
        }

        #endregion

        #region [ Window ]
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (this.Changed)
            {
                MessageBoxResult result = MessageBox.Show("Current work is not saved!", AppName, MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        {
                            e.Cancel = true;
                            break;
                        }

                    case MessageBoxResult.Yes:
                        {
                            CheckAndSave();
                            this.Kill();
                            break;
                        }

                    case MessageBoxResult.No:
                        {
                            this.Kill();
                            break;
                        }
                }
            }
            else
            {
                this.Kill();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetSize();
        } 
        #endregion

        #region [ Timer ]
        void beautifyTimer_Tick(object sender, EventArgs e)
        {
            if (!this.Beutified)
            {
                Beautify();
            }
        } 
        #endregion
        #endregion

        #region [ Methods ]
        #region [ Window ]
        /// <summary>
        /// Set the main window title.
        /// </summary>
        private void SetTitle()
        {
            string title = AppName;

            if (this.BFDialect != null)
            {
                title += " - " + BFDialect.Name;

                if (this.FileName == null)
                {
                    title += " - Noname";
                }
                else
                {
                    title += " - " + this.FileName;
                }

                title += this.BFDialect.Extension;

                if (this.Changed)
                {
                    title += "*";
                }
            }

            this.Title = title;
        }

        /// <summary>
        /// Ensures all windows are closed.
        /// </summary>
        private void Kill()
        {
            SaveSettings();
            wmv.Kill();
            stdoutView.Kill();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Creates helper windows, reads and sets their sizes.
        /// </summary>
        private void SetSize()
        {
            stdoutView = new STDOUTView();
            wmv = new WindowMemoryView();
            wmv.ClickPreventer = System.Windows.Visibility.Collapsed;
            DataRow[] settingsRow;

            if (File.Exists(settingsFile))
            {
                LoadSettings();
            }
            else
            {
                CreateDummySettings();
            }

            if (settingsDS.Tables.Contains(WindowsSizes))
            {
                settingsRow = this.settingsDS.Tables[WindowsSizes].Select("WindowName='" + AppName + "'");
                if (settingsRow != null)
                {
                    this.Width = Convert.ToDouble(settingsRow[0]["Width"]);
                    this.Height = Convert.ToDouble(settingsRow[0]["Height"]);
                    this.Left = Convert.ToDouble(settingsRow[0]["Left"]);
                    this.Top = Convert.ToDouble(settingsRow[0]["Top"]);
                }

                settingsRow = this.settingsDS.Tables[WindowsSizes].Select("WindowName='" + this.wmv.Title + "'");
                if (settingsRow != null)
                {
                    //this.wmv.Width = Convert.ToDouble(settingsRow[0]["Width"]);
                    this.wmv.Height = Convert.ToDouble(settingsRow[0]["Height"]);
                    this.wmv.Left = Convert.ToDouble(settingsRow[0]["Left"]);
                    this.wmv.Top = Convert.ToDouble(settingsRow[0]["Top"]);
                }

                settingsRow = this.settingsDS.Tables[WindowsSizes].Select("WindowName='" + this.stdoutView.Title + "'");
                if (settingsRow != null)
                {
                    this.stdoutView.Width = Convert.ToDouble(settingsRow[0]["Width"]);
                    this.stdoutView.Height = Convert.ToDouble(settingsRow[0]["Height"]);
                    this.stdoutView.Left = Convert.ToDouble(settingsRow[0]["Left"]);
                    this.stdoutView.Top = Convert.ToDouble(settingsRow[0]["Top"]);
                }
            }

            wmv.Show();
            stdoutView.Show();
        }
        #endregion

        #region [ BF Dialect ]
        /// <summary>
        /// Sets the current bf dialect according to the file name (extension).
        /// </summary>
        private void SetDialectByExtension(string Extension)
        {
            this.BFDialect = BFDialects.Dialects.First(bfd => bfd.Extension.Equals(Extension));
            this.rtfBox.IsEnabled = true;
        }

        /// <summary>
        /// Sets the current bf dialect according to the dialect name.
        /// </summary>
        /// <param name="DialectName">string containing the dialect name</param>
        private void SetDialectByName(string DialectName)
        {
            this.BFDialect = BFDialects.Dialects.First(bfd => bfd.Name.Equals(DialectName));
            this.rtfBox.IsEnabled = true;
        }

        /// <summary>
        /// Calls the dialect selector window.
        /// </summary>
        private void ChangeDialect()
        {
            DialectSelector ds = new DialectSelector();
            ds.Owner = this;
            bool result = (bool)ds.ShowDialog();
            if (result)
            {
                //SetDialectByName(ds.Dialect);
                this.BFDialect = ds.Dialect;
                TranslateDialect();
                this.rtfBox.IsEnabled = true;
            }
        }

        /// <summary>
        /// Changes the programm element chunks to the current dialect.
        /// </summary>
        private void TranslateDialect()
        {
            FlowDocument fd = new FlowDocument();
            foreach (Paragraph p in this.rtfBox.Document.Blocks)
            {
                Paragraph newParagraph = new Paragraph();
                foreach (Run r in p.Inlines)
                {
                    Run newRun = new Run();
                    newRun.Foreground = r.Foreground;
                    newRun.Background = r.Background;
                    newRun.Text = r.Text;
                    newRun.Tag = r.Tag;
                    newRun.Name = r.Name;
                    if (r.Tag != null)
                    {
                        ProgrammElement pe = (ProgrammElement)r.Tag;
                        newRun.Text = BFDialect.DialectItems.First(di => di.Command == pe.Command).DialectText;
                    }
                    newParagraph.Inlines.Add(newRun);
                }
                fd.Blocks.Add(newParagraph);
            }

            this.rtfBox.Document = fd;
        } 
        #endregion

        #region [ Save BF Source ]
        /// <summary>
        /// Get file extension and file name separately
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns>array of two strings - filename and extension.</returns>
        private string[] GetExtensionAndFileName(string FileName)
        {
            string[] splitter = FileName.Split(new char[] { '.' });
            string[] result = new string[2];

            result[1] = "." + splitter[splitter.Length - 1];
            result[0] = FileName.Substring(0, FileName.Length - result[1].Length);

            return result;
        }

        /// <summary>
        /// Save As logic.
        /// </summary>
        private void SaveSourceAs()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension
            string filter = "";

            foreach (BrainFuckDialect bfd in BFDialects.Dialects)
            {
                filter += bfd.Name + " documents (" + bfd.Extension + ")|*" + bfd.Extension + "|";
            }
            dlg.Filter = filter.Substring(0, filter.Length - 1);
            dlg.DefaultExt = BFDialects.Dialects[0].Extension;

            if ((bool)dlg.ShowDialog())
            {
                string[] parts = GetExtensionAndFileName(dlg.FileName);
                this.FileName = parts[0];
                SetDialectByExtension(parts[1]);
                TranslateDialect();
                SaveSource();
                this.Changed = false;
            }
        }

        /// <summary>
        /// Save logic.
        /// </summary>
        private void SaveSource()
        {
            // Save document
            List<string> lines = new List<string>();
            foreach (Paragraph p in this.rtfBox.Document.Blocks)
            {
                string line = "";
                foreach (Run r in p.Inlines)
                {
                    if (r.Text != null)
                    {
                        line += r.Text;
                    }
                }
                lines.Add(line);
            }
            File.WriteAllLines(this.FileName + this.BFDialect.Extension, lines.ToArray());
            this.Changed = false;
        }

        /// <summary>
        /// Decision if save needed and which is to be performed.
        /// </summary>
        private void CheckAndSave()
        {
            if (this.Changed)
            {
                if (this.FileName == null)
                {
                    SaveSourceAs();
                }
                else
                {
                    SaveSource();
                }
            }
        } 
        #endregion

        #region [ Run BF ]
        /// <summary>
        /// Interpretes the bf source - in run once or step-by-step mode
        /// </summary>
        /// <param name="mode">bool run mode - step-by-step if true</param>
        private void RunProgram(bool mode)
        {
            #region [ Preparations ]
            this.changeEventsPermited = true;
            beautifyTimer.Stop();
            Beautify();
            this.changeEventsPermited = false;
            wmv.InitializeMemory();

            int source_pos = 0;
            int new_source_pos = 0;
            bool jump = false;
            bool jumpOver = false;
            Stack<int> stack = new Stack<int>();
            Dictionary<int, int> brackets = new Dictionary<int, int>();

            stdoutView.STDOUT = "";

            // Refresh source code.
            sourceCode = new List<Run>();
            foreach (Paragraph p in this.rtfBox.Document.Blocks)
            {
                foreach (Run r in p.Inlines)
                {
                    if (r.Name.Equals("Command"))
                    {
                        sourceCode.Add(r);
                    }
                }
            }
            #endregion

            #region [ Fill brackets dictionary ]
            for (int i = 0; i < sourceCode.Count; i++)
            {
                ProgrammElement pe = (ProgrammElement)sourceCode[i].Tag;
                switch (pe.Command)
                {
                    case BrainFuckCommand.Loop_Head_Condition:
                        {
                            stack.Push(i);
                            break;
                        }

                    case BrainFuckCommand.Loop_Tail_Condition:
                        {
                            brackets.Add(stack.Pop(), i);
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }
            }

            var sortedBrackets = brackets.OrderBy(br => br.Key);
            brackets = new Dictionary<int, int>();
            foreach (var item in sortedBrackets)
            {
                brackets.Add(item.Key, item.Value);
            }

            //foreach (var item in brackets)
            //{
            //    stdoutView.STDOUT += string.Format("Brake opened at {0} and closed at {1}", item.Key, item.Value) + System.Environment.NewLine;
            //}


            if (stack.Count == 0)
            {
                //stdoutView.STDOUT += string.Format("Brackets are plausible.") + System.Environment.NewLine;
            }
            else
            {
                List<int> unclosedBrackets = new List<int>();

                while (stack.Count > 0)
                {
                    source_pos = stack.Pop();
                    stdoutView.STDOUT += string.Format("Unclosed bracket at {0}", source_pos) + System.Environment.NewLine;
                    unclosedBrackets.Add(source_pos);
                }

                for (int i = 0; i < sourceCode.Count; i++)
                {
                    if (unclosedBrackets.Contains(i))
                    {
                        stdoutView.STDOUT += string.Format(" ==>{0}<== ", sourceCode[i].Text);
                    }
                    else
                    {
                        stdoutView.STDOUT += sourceCode[i].Text;
                    }
                }
                stdoutView.STDOUT += System.Environment.NewLine;
                stdoutView.STDOUT += string.Format("Brackets are inconsistent! End!");
                return;
            }

            stdoutView.STDOUT += System.Environment.NewLine;
            #endregion

            #region [ MAIN LOOP ]
            while (true)
            {
                changeEventsPermited = false;
                jump = false;
                jumpOver = false;

                ProgrammElement pe = (ProgrammElement)sourceCode[source_pos].Tag;
                sourceCode[source_pos].BringIntoView();

                if (pe.Breakpoint)
                {
                    changeEventsPermited = false;
                    sourceCode[source_pos].BringIntoView(); 
                    sourceCode[source_pos].Background = Brushes.Tomato;

                    RunStep rs = new RunStep("Visual DingsBums - Breakpoint");
                    rs.Owner = this;
                    rs.Hint = "Breakpoint: " + pe.Command.ToString();
                    rs.ShowDialog();
                    switch (rs.Result)
                    {
                        case VDBOBJECTS.StepMode.Cancel:
                            {
                                this.changeEventsPermited = true;
                                beautifyTimer.Start();
                                return;
                            }
                        case VDBOBJECTS.StepMode.NextStep:
                            {
                                mode = true;
                                break;
                            }
                        case VDBOBJECTS.StepMode.Run:
                            {
                                mode = false;
                                break;
                            }
                    }
                    sourceCode[source_pos].Background = Brushes.DarkOrange;
                }
                else if (mode)
                {
                    changeEventsPermited = false;
                    sourceCode[source_pos].Background = Brushes.DarkGray;
                    sourceCode[source_pos].BringIntoView();

                    RunStep rs = new RunStep("Visual DingsBums - Run Step Mode");
                    rs.Hint = "Next step: " + pe.Command.ToString();
                    rs.Owner = this;
                    rs.ShowDialog();
                    switch (rs.Result)
                    {
                        case VDBOBJECTS.StepMode.Cancel:
                            {
                                changeEventsPermited = true;
                                beautifyTimer.Start();
                                return;
                            }
                        case VDBOBJECTS.StepMode.NextStep:
                            {
                                mode = true;
                                break;
                            }
                        case VDBOBJECTS.StepMode.Run:
                            {
                                mode = false;
                                break;
                            }
                    }
                    sourceCode[source_pos].Background = Brushes.Transparent;
                }

                switch (pe.Command)
                {
                    case BrainFuckCommand.Move_to_next_cell:
                        {
                            wmv.Next();
                            break;
                        }
                    case BrainFuckCommand.Move_to_previous_cell:
                        {
                            if (!wmv.Previous())
                            {
                                changeEventsPermited = true;
                                beautifyTimer.Start();
                                return;
                            }
                            break;
                        }
                    case BrainFuckCommand.Increment_cell:
                        {
                            wmv.Increment();
                            break;
                        }
                    case BrainFuckCommand.Decrement_cell:
                        {
                            wmv.Decrement();
                            break;
                        }
                    case BrainFuckCommand.Output_cell:
                        {
                            stdoutView.STDOUT += wmv.ToStdOUT();
                            break;
                        }
                    case BrainFuckCommand.Input_cell:
                        {
                            STDINView stdin = new STDINView();
                            stdin.Owner = this;
                            stdin.ShowDialog();
                            wmv.FromStdIN(stdin.Result);
                            stdin.Close();
                            break;
                        }
                    case BrainFuckCommand.Loop_Head_Condition:
                        {
                            if (wmv.CurrentItem.Value == 0)
                            {
                                // Jump after loop end.
                                if (brackets.TryGetValue(source_pos, out new_source_pos))
                                {
                                    jump = true;
                                    new_source_pos++;
                                }
                                else
                                {
                                    // Critical error - bracket unknown.
                                    stdoutView.STDOUT += "Unknown bracket!" + System.Environment.NewLine;
                                    return;
                                }
                            }
                            break;
                        }

                    case BrainFuckCommand.Loop_Tail_Condition:
                        {
                            if (!(wmv.CurrentItem.Value == 0))
                            {
                                // Jump back to loop header.
                                new_source_pos = brackets.First(br => br.Value == source_pos).Key;
                                jump = true;
                            }
                            break;
                        }

                    default:
                        {
                            jumpOver = true;
                            break;
                        }
                }

                if (jump)
                {
                    source_pos = new_source_pos;
                }
                else
                {
                    source_pos++;
                }


                if ((source_pos < 0) || (source_pos > sourceCode.Count - 1))
                {
                    break;
                }

                if (jumpOver)
                {
                    continue;
                }
            }
            #endregion

            this.changeEventsPermited = true;
            this.beautifyTimer.Start();
        } 
        #endregion

        #region [ BF Source highlighting ]
        /// <summary>
        /// Retrieves the caret position
        /// </summary>
        private void CurrentCaretPosition()
        {
            //if (!Beutifying)
            //{
            //    TextPointer caretPos = this.rtfBox.CaretPosition;
            //    TextPointer caretLine = this.rtfBox.CaretPosition.GetLineStartPosition(0);

            //    currentLineNumber = Convert.ToInt32(caretLine.Paragraph.Tag);
            //    currentColumnNumber = caretLine.GetOffsetToPosition(caretPos);
            //    currentOffset = caretPos.GetOffsetToPosition(caretPos.DocumentStart);

            //    this.TextBlockStatus.Text = string.Format("Line: {0}, Column: {1}", currentLineNumber, currentColumnNumber);
            //}

            if (!Beutifying)
            {
                int currentLineNumber = 0;
                TextPointer caretPos = this.rtfBox.CaretPosition;
                TextPointer caretLineStart = this.rtfBox.CaretPosition.GetLineStartPosition(0);
                currentOffset = this.rtfBox.Document.ContentStart.GetOffsetToPosition(this.rtfBox.CaretPosition);

                foreach (Paragraph p in this.rtfBox.Document.Blocks)
                {
                    currentLineNumber++;
                    if (p.Equals(caretLineStart.Paragraph))
                    {
                        break;
                    }
                }

                int currentColumnNumber = Math.Max(caretLineStart.GetOffsetToPosition(caretPos) - 1, 0);
                this.TextBlockStatus.Text = string.Format("Line: {0}, Column: {1}, Offset: {2}", currentLineNumber, currentColumnNumber, currentOffset);
            }
        }

        /// <summary>
        /// Returns the color of given brush as 32 Bit integer.
        /// </summary>
        /// <param name="brush">Brush to be converted.</param>
        /// <returns></returns>
        private int ColorFromBrush(Brush brush)
        {
            if (brush == null)
            {
                return 0;
            }
            else
            {
                Color color = ((SolidColorBrush)brush).Color;
                byte[] channel = new byte[4];
                channel[0] = color.A;
                channel[1] = color.R;
                channel[2] = color.G;
                channel[3] = color.B;

                return BitConverter.ToInt32(channel, 0);
            }
        }

        /// <summary>
        /// Ensures proper coloring of recognized language elements.
        /// </summary>
        private void Beautify()
        {
            #region [ Abort on empty document ]
            if (rtfBox.Document.Blocks.Count < 1)
            {
                Beutifying = false;
                Beutified = true;
                return;
            } 
            #endregion

            #region [ Set application state ]
            this.changeEventsPermited = false;
            Beutifying = true;
            #endregion

            #region [ Fields ]
            Run cR;
            FlowDocument newDocument = new FlowDocument();
            int commandCounter = 0;
            int lastOffset = currentOffset;
            #endregion

            #region [ Document recreation ]
            int LineCounter = 0;
            foreach (Paragraph p in rtfBox.Document.Blocks)
            {
                #region [ Init loop run ]
                Paragraph newParagraph = new Paragraph();
                newParagraph.Tag = ++LineCounter;

                List<int> breakpoints = new List<int>();
                #endregion

                #region [ Collect paragraph contents ]
                string workText = "";

                foreach (Inline i in p.Inlines)
                {
                    cR = (Run)i;
                    if (cR.Tag != null)
                    {
                        ProgrammElement pe = (ProgrammElement)cR.Tag;
                        if (pe.Breakpoint)
                        {
                            breakpoints.Add(pe.Position);
                        }
                    }
                    if (cR.Text.Length > 0)
                    {
                        workText += cR.Text;
                    }
                }
                #endregion

                #region [ Search for command ]
                string charCollector = "";
                foreach (char c in workText)
                {
                    charCollector += c;

                    foreach (BrainFuckLanguageItem bfli in BFDialect.DialectItems)
                    {
                        // Command was found
                        if (charCollector.EndsWith(bfli.DialectText))
                        {
                            #region [ Set any additional text as comment ]
                            if (charCollector.Length > bfli.DialectText.Length)
                            {
                                newParagraph.Inlines.Add(CloseComment(charCollector.Substring(0, charCollector.Length - bfli.DialectText.Length)));
                            }
                            charCollector = "";
                            #endregion

                            #region [ Create new run  with command ]
                            Run newRun = new Run();
                            newRun.Text = bfli.DialectText;
                            newRun.FontStyle = bfli.FontStyle;
                            newRun.Foreground = bfli.Foreground;
                            newRun.FontWeight = bfli.FontWeight;
                            newRun.Name = "Command";

                            #region [ Create fancy tooltip ]
                            Border border = new Border();
                            border.BorderBrush = Brushes.Black;
                            border.Padding = new Thickness(10);
                            border.Margin = new Thickness(0);
                            border.BorderThickness = new Thickness(2);
                            border.CornerRadius = new CornerRadius(5);
                            border.Background = Brushes.White;

                            Grid grd = new Grid();
                            grd.ColumnDefinitions.Add(new ColumnDefinition());
                            grd.ColumnDefinitions.Add(new ColumnDefinition());
                            grd.ColumnDefinitions.Add(new ColumnDefinition());
                            grd.ColumnDefinitions[0].Width = GridLength.Auto;
                            grd.ColumnDefinitions[1].Width = new GridLength(20);
                            grd.ColumnDefinitions[2].Width = GridLength.Auto;
                            Image img = new Image();
                            img.Source = bfli.Picture;
                            img.Height = 16;
                            img.Width = 16;
                            Grid.SetColumn(img, 0);
                            grd.Children.Add(img);
                            TextBlock txt = new TextBlock();
                            txt.Text = bfli.Command.ToString();
                            txt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                            Grid.SetColumn(txt, 2);
                            grd.Children.Add(txt);
                            border.Child = grd; 
                            #endregion

                            newRun.ToolTip = border;//= bfli.Command.ToString();
                            newParagraph.Inlines.Add(newRun);
                            newRun.Tag = new ProgrammElement(bfli.Command, ++commandCounter, false);
                            #endregion
                        }
                    }
                }
                #endregion

                #region [ Restore breakpoints ]
                if (breakpoints.Count > 0)
                {
                    foreach (Inline i in newParagraph.Inlines)
                    {
                        cR = (Run)i;
                        if (cR.Tag != null)
                        {
                            ProgrammElement pe = (ProgrammElement)cR.Tag;

                            if (breakpoints.Any(bp => bp == pe.Position))
                            {
                                pe.Breakpoint = true;
                                cR.Background = Brushes.Orange;
                            }
                        }
                        if (cR.Text.Length > 0)
                        {
                            workText += cR.Text;
                        }
                    }
                }
                #endregion

                #region [ Set any text left as comment ]
                if (charCollector.Length > 0)
                {
                    newParagraph.Inlines.Add(CloseComment(charCollector));
                    charCollector = "";
                }
                #endregion

                #region [ Add prepared paragraph ]
                if (newParagraph.Inlines.Count > 0)
                {
                    newDocument.Blocks.Add(newParagraph);
                }
                #endregion
            }

            if (newDocument.Blocks.Count > 0)
            {
                this.rtfBox.Document = newDocument;
            }

            this.rtfBox.IsDocumentEnabled = true;
            #endregion

            #region [ Restore caret position ]
            //TextPointer tp = this.rtfBox.Document.ContentStart;
            //TextPointer np = tp.GetNextInsertionPosition(LogicalDirection.Forward);

            //if (np != null)
            //{
            //    while (true)
            //    {
            //        TextPointer next = np.GetNextInsertionPosition(LogicalDirection.Forward);
            //        if (next == null)
            //        {
            //            break;
            //        }

            //        int lineNr = Convert.ToInt32(next.Paragraph.Tag);
            //        if (lineNr == currentLineNumber)
            //        {
            //            break;
            //        }

            //        np = next;
            //    }
            //    tp = np.GetPositionAtOffset(currentColumnNumber);
            //    this.rtfBox.Selection.Select(tp, tp);
            //} 

            TextPointer tp = this.rtfBox.Document.ContentStart;
            TextPointer np = tp.GetNextInsertionPosition(LogicalDirection.Forward);

            if (np != null)
            {
                int co = tp.GetOffsetToPosition(np);
                while (co < lastOffset)
                {
                    TextPointer next = np.GetNextInsertionPosition(LogicalDirection.Forward);
                    if (next == null)
                    {
                        break;
                    }
                    co = tp.GetOffsetToPosition(next);
                    np = next;
                }
                this.rtfBox.Selection.Select(np, np);
            }
            #endregion

            #region [ Restore application state ]
            Beutifying = false;
            Beutified = true;
            this.changeEventsPermited = true; 
            #endregion
        }

        /// <summary>
        /// Creates new comment chunk from string.
        /// </summary>
        /// <param name="Comment">string with comment content</param>
        /// <returns>Run with respective formattings</returns>
        private Run CloseComment(string Comment)
        {
            Run newRun = new Run();
            newRun.Text = Comment;
            newRun.Foreground = Brushes.LightGray;
            newRun.FontStyle = FontStyles.Italic;
            newRun.Name = "Comment";
            return newRun;
        }

        /// <summary>
        /// Switches break point on / off
        /// </summary>
        private void ToggleBreakpoint()
        {
            //TextPointer caretLineStart = this.rtfBox.CaretPosition.GetLineStartPosition(0);
            TextPointer caretLineStart = this.rtfBox.CaretPosition;

            if (caretLineStart.Parent is Run)
            {
                Run currentRun = (Run)caretLineStart.Parent;
                if (currentRun.Name.Equals("Command"))
                {
                    ProgrammElement pe = (ProgrammElement)currentRun.Tag;
                    if (pe.Breakpoint)
                    {
                        pe.Breakpoint = false;
                        currentRun.Background = Brushes.Transparent;
                    }
                    else
                    {
                        pe.Breakpoint = true;
                        currentRun.Background = Brushes.DarkOrange;
                    }
                    currentRun.Tag = pe;
                }
            }

            //bool commandFound = false;
            //foreach (Inline i in caretLineStart.Paragraph.Inlines)
            //{
            //    if (i.Name != null)
            //    {
            //        if (i.Name.Equals("Command"))
            //        {
            //            commandFound = true;
            //            break;
            //        }
            //    }
            //}

            //if (commandFound)
            //{
            //    if (caretLineStart.Paragraph.Tag == null)
            //    {
            //        this.changePermited = false;
            //        caretLineStart.Paragraph.Background = Brushes.DarkOrange;
            //        caretLineStart.Paragraph.Tag = "Breakpoint";
            //        this.changePermited = true;
            //    }
            //    else
            //    {
            //        this.changePermited = false;
            //        caretLineStart.Paragraph.Background = Brushes.Transparent;
            //        caretLineStart.Paragraph.Tag = null;
            //        this.changePermited = true;
            //    }
            //}
        } 
        #endregion

        #region [ Settings ]
        /// <summary>
        /// Creates new standart settings file when not existing.
        /// </summary>
        private void CreateDummySettings()
        {
            DataTable dt;
            DataTable sz;

            if (BFDialects.Dialects == null)
            {
                BFDialects.Dialects = new System.Collections.ObjectModel.ObservableCollection<BrainFuckDialect>();
            }

            BrainFuckDialect bfd = new BrainFuckDialect("Marsupilamian", ".marsu", "Houba!", "Houbaaa!", "Houbi!", "Houbibi!", "Houba?", "Hop!", "Marsu?", "Marsu!");
            bfd.DialectItems[(int)BrainFuckCommand.Move_to_previous_cell].Foreground = Brushes.Red;
            bfd.DialectItems[(int)BrainFuckCommand.Move_to_next_cell].Foreground = Brushes.Red;
            bfd.DialectItems.First(bf => bf.Command == BrainFuckCommand.Decrement_cell).Foreground = Brushes.DarkMagenta;
            BFDialects.Dialects.Add(bfd);

            bfd = new BrainFuckDialect("Ook!", ".ook", "Ook. Ook?", "Ook? Ook.", "Ook. Ook.", "Ook! Ook!", "Ook! Ook?", "Ook? Ook!", "Ook. Ook!", "Ook! Ook.");
            bfd.DialectItems[(int)BrainFuckCommand.Move_to_previous_cell].Foreground = Brushes.Red;
            bfd.DialectItems[(int)BrainFuckCommand.Move_to_next_cell].Foreground = Brushes.Red;
            bfd.DialectItems.First(bf => bf.Command == BrainFuckCommand.Decrement_cell).Foreground = Brushes.DarkMagenta;
            BFDialects.Dialects.Add(bfd);

            bfd = new BrainFuckDialect();
            bfd.DialectItems[(int)BrainFuckCommand.Move_to_previous_cell].Foreground = Brushes.Red;
            bfd.DialectItems[(int)BrainFuckCommand.Move_to_next_cell].Foreground = Brushes.Red;
            bfd.DialectItems.First(bf => bf.Command == BrainFuckCommand.Decrement_cell).Foreground = Brushes.DarkMagenta;
            BFDialects.Dialects.Add(bfd);

            settingsDS = new DataSet(AppName);
            dt = new DataTable(Dialects);
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("DialectName", typeof(string));
            dt.Columns.Add("FileExtension", typeof(string));
            for (byte i = 0; i < 8; i++)
            {
                BrainFuckCommand bfc = (BrainFuckCommand)Enum.ToObject(typeof(BrainFuckCommand), i);
                dt.Columns.Add(bfc.ToString(), typeof(string));
                dt.Columns.Add(bfc.ToString() + "_Fontstyle", typeof(string));
                dt.Columns.Add(bfc.ToString() + "_Foreground", typeof(int));
                dt.Columns.Add(bfc.ToString() + "_Fontweight", typeof(string));
            }

            int ID = 0;
            foreach (BrainFuckDialect bfdi in BFDialects.Dialects)
            {
                DataRow row = dt.NewRow();
                row["ID"] = ID++;
                row["DialectName"] = bfdi.Name;
                row["FileExtension"] = bfdi.Extension;
                foreach (BrainFuckLanguageItem bfli in bfdi.DialectItems)
                {
                    row[bfli.Command.ToString()] = bfli.DialectText;
                    row[bfli.Command.ToString() + "_Fontstyle"] = bfli.FontStyle.ToString();
                    row[bfli.Command.ToString() + "_Foreground"] = ColorFromBrush(bfli.Foreground);
                    row[bfli.Command.ToString() + "_Fontweight"] = bfli.FontWeight.ToString();
                }
                dt.Rows.Add(row);
            }


            sz = new DataTable(WindowsSizes);
            sz.Columns.Add("ID", typeof(int));
            sz.Columns.Add("WindowName", typeof(string));
            sz.Columns.Add("Left", typeof(double));
            sz.Columns.Add("Top", typeof(double));
            sz.Columns.Add("Width", typeof(double));
            sz.Columns.Add("Height", typeof(double));

            sz.Rows.Add(0, AppName, 0, 0, 1200, 720);
            sz.Rows.Add(1, wmv.Title, 1201, 0, -1, 970);
            sz.Rows.Add(2, stdoutView.Title, 0, 725, 1200, 250);

            settingsDS.Tables.Add(dt);
            settingsDS.Tables.Add(sz);
            settingsDS.WriteXml(settingsFile);
        }

        /// <summary>
        /// Loads the settings from file.
        /// </summary>
        private void LoadSettings()
        {
            this.settingsDS = new DataSet();
            settingsDS.ReadXml(settingsFile);

            BFDialects.Dialects = new System.Collections.ObjectModel.ObservableCollection<BrainFuckDialect>();
            if (settingsDS.Tables.Contains(Dialects))
            {
                foreach (DataRow row in settingsDS.Tables[Dialects].Rows)
                {
                    BFDialects.Dialects.Add(new BrainFuckDialect(row));
                }
            }
        }

        /// <summary>
        /// Saves the application's windows sizes and positions.
        /// </summary>
        private void SaveSettings()
        {
            DataTable dt;
            DataTable sz;

            settingsDS = new DataSet(AppName);
            dt = new DataTable(Dialects);
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("DialectName", typeof(string));
            dt.Columns.Add("FileExtension", typeof(string));
            for (byte i = 0; i < 8; i++)
            {
                BrainFuckCommand bfc = (BrainFuckCommand)Enum.ToObject(typeof(BrainFuckCommand), i);
                dt.Columns.Add(bfc.ToString(), typeof(string));
                dt.Columns.Add(bfc.ToString() + "_Fontstyle", typeof(string));
                dt.Columns.Add(bfc.ToString() + "_Foreground", typeof(int));
                dt.Columns.Add(bfc.ToString() + "_Fontweight", typeof(string));
            }

            int ID = 0;
            foreach (BrainFuckDialect bfdi in BFDialects.Dialects)
            {
                DataRow row = dt.NewRow();
                row["ID"] = ID++;
                row["DialectName"] = bfdi.Name;
                row["FileExtension"] = bfdi.Extension;
                foreach (BrainFuckLanguageItem bfli in bfdi.DialectItems)
                {
                    row[bfli.Command.ToString()] = bfli.DialectText;
                    row[bfli.Command.ToString() + "_Fontstyle"] = bfli.FontStyle.ToString();
                    row[bfli.Command.ToString() + "_Foreground"] = ColorFromBrush(bfli.Foreground);
                    row[bfli.Command.ToString() + "_Fontweight"] = bfli.FontWeight.ToString();
                }
                dt.Rows.Add(row);
            }

            sz = new DataTable(WindowsSizes);
            sz.Columns.Add("ID", typeof(int));
            sz.Columns.Add("WindowName", typeof(string));
            sz.Columns.Add("Left", typeof(double));
            sz.Columns.Add("Top", typeof(double));
            sz.Columns.Add("Width", typeof(double));
            sz.Columns.Add("Height", typeof(double));

            sz.Rows.Add(0, AppName, this.Left, this.Top, this.Width, this.Height);
            sz.Rows.Add(1, wmv.Title, wmv.Left, wmv.Top, -1, wmv.Height);
            sz.Rows.Add(2, stdoutView.Title, stdoutView.Left, stdoutView.Top, stdoutView.Width, stdoutView.Height);

            settingsDS.Tables.Add(dt);
            settingsDS.Tables.Add(sz);
            settingsDS.WriteXml(settingsFile);
        }
        #endregion
        #endregion
    }
}
