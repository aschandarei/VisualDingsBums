using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.ComponentModel;
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
				return _filename;
			}

			set
			{
				_filename = value;
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
				return _bfDialect;
			}

			set
			{
				_bfDialect = value;
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
				return _changed;
			}

			set
			{
				_changed = value;
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
				if (!HasDocument) return false;

				sourceCode =
				(
					from Paragraph p in rtfBox.Document.Blocks.Cast<Paragraph>()
					from Run r in p.Inlines.Cast<Run>()
					where r.Name.Equals("Command")
					select r
				).ToList();

				return sourceCode.Count < 1;
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
			beautifyTimer = new DispatcherTimer
			{
				Interval = new TimeSpan(0, 0, 5)
			};
			beautifyTimer.Tick += beautifyTimer_Tick;
		}
		#endregion

		#region [ Events ]
		#region [ File commands ]
		#region [ New ]
		private void FileNew_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			rtfBox.Document = new FlowDocument();
			ChangeDialect();
			beautifyTimer.Start();
			HasDocument = true;
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
				FileName = parts[0];
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
				rtfBox.Document = fd;

				Beautify();
				Changed = false;
				HasDocument = true;
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
			e.CanExecute = Changed;
		}
		#endregion

		#region [ SaveAs ]
		private void FileSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			SaveSourceAs();
		}

		private void FileSaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = Changed;
		}
		#endregion

		#region [ Application quit ]
		private void AppQuit_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
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
			RunProgram(true);
		}

		private void RunStep_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CanRun;
		}

		private void RunFull_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			RunProgram(false);
		}

		private void RunFull_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CanRun;
		}
		#endregion

		#region [ Breakpoints ]
		private void ToggleBreakpoint_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleBreakpoint();
		}

		private void ToggleBreakpoint_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = HasDocument;
		}

		private void ClearAllBreakpoints_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			foreach (Paragraph p in rtfBox.Document.Blocks)
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
			e.CanExecute = HasDocument;
		}
		#endregion

		private void mnuBrainVixx_Click(object sender, RoutedEventArgs e)
		{
			if (!rtfBox.IsEnabled)
			{
				rtfBox.Document = new FlowDocument();
				ChangeDialect();
				rtfBox.IsEnabled = true;
				HasDocument = true;
			}

			BrainVixx bv = new BrainVixx(wmv, stdoutView, BFDialect);
			bv.Owner = this;
			if ((bool)bv.ShowDialog())
			{
				Paragraph newParagraph = new Paragraph();
				foreach (Run r in bv.Runs)
				{
					newParagraph.Inlines.Add(r);
				}
				rtfBox.Document.Blocks.Add(newParagraph);

				Beautify();
				beautifyTimer.Start();
				Changed = true;
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
			e.CanExecute = HasDocument;
		}
		#endregion

		#region [ Rich text box ]
		private void rtfBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (changeEventsPermited)
			{
				Changed = true;
				Beutified = false;
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
			if (Changed)
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
							Kill();
							break;
						}

					case MessageBoxResult.No:
						{
							Kill();
							break;
						}
				}
			}
			else
			{
				Kill();
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
			if (!Beutified)
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
			var title = AppName;

			if (BFDialect != null)
			{
				title += " - " + BFDialect.Name;

				if (FileName == null)
				{
					title += " - Noname";
				}
				else
				{
					title += " - " + FileName;
				}

				title += BFDialect.Extension;

				if (Changed)
				{
					title += "*";
				}
			}

			Title = title;
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
				settingsRow = settingsDS.Tables[WindowsSizes].Select("WindowName='" + AppName + "'");
				if (settingsRow != null)
				{
					Width = Convert.ToDouble(settingsRow[0]["Width"]);
					Height = Convert.ToDouble(settingsRow[0]["Height"]);
					Left = Convert.ToDouble(settingsRow[0]["Left"]);
					Top = Convert.ToDouble(settingsRow[0]["Top"]);
				}

				settingsRow = settingsDS.Tables[WindowsSizes].Select("WindowName='" + wmv.Title + "'");
				if (settingsRow != null)
				{
					//this.wmv.Width = Convert.ToDouble(settingsRow[0]["Width"]);
					wmv.Height = Convert.ToDouble(settingsRow[0]["Height"]);
					wmv.Left = Convert.ToDouble(settingsRow[0]["Left"]);
					wmv.Top = Convert.ToDouble(settingsRow[0]["Top"]);
				}

				settingsRow = settingsDS.Tables[WindowsSizes].Select("WindowName='" + stdoutView.Title + "'");
				if (settingsRow != null)
				{
					stdoutView.Width = Convert.ToDouble(settingsRow[0]["Width"]);
					stdoutView.Height = Convert.ToDouble(settingsRow[0]["Height"]);
					stdoutView.Left = Convert.ToDouble(settingsRow[0]["Left"]);
					stdoutView.Top = Convert.ToDouble(settingsRow[0]["Top"]);
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
			BFDialect = BFDialects.Dialects.First(bfd => bfd.Extension.Equals(Extension));
			rtfBox.IsEnabled = true;
		}

		/// <summary>
		/// Sets the current bf dialect according to the dialect name.
		/// </summary>
		/// <param name="DialectName">string containing the dialect name</param>
		private void SetDialectByName(string DialectName)
		{
			BFDialect = BFDialects.Dialects.First(bfd => bfd.Name.Equals(DialectName));
			rtfBox.IsEnabled = true;
		}

		/// <summary>
		/// Calls the dialect selector window.
		/// </summary>
		private void ChangeDialect()
		{
			var ds = new DialectSelector
			{
				Owner = this
			};
			var result = (bool)ds.ShowDialog();
			if (result)
			{
				//SetDialectByName(ds.Dialect);
				BFDialect = ds.Dialect;
				TranslateDialect();
				rtfBox.IsEnabled = true;
			}
		}

		/// <summary>
		/// Changes the programm element chunks to the current dialect.
		/// </summary>
		private void TranslateDialect()
		{
			var fd = new FlowDocument();
			foreach (Paragraph p in rtfBox.Document.Blocks.Cast<Paragraph>())
			{
				var newParagraph = new Paragraph();
				foreach (Run r in p.Inlines.Cast<Run>())
				{
					var newRun = new Run
					{
						Foreground = r.Foreground,
						Background = r.Background,
						Text = r.Text,
						Tag = r.Tag,
						Name = r.Name
					};
					if (r.Tag != null)
					{
						var pe = (ProgrammElement)r.Tag;
						newRun.Text = BFDialect.DialectItems.First(di => di.Command == pe.Command).DialectText;
					}
					newParagraph.Inlines.Add(newRun);
				}
				fd.Blocks.Add(newParagraph);
			}

			rtfBox.Document = fd;
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
			var splitter = FileName.Split(new char[] { '.' });
			var result = new string[2];

			result[1] = "." + splitter[splitter.Length - 1];
			result[0] = FileName.Substring(0, FileName.Length - result[1].Length);

			return result;
		}

		/// <summary>
		/// Save As logic.
		/// </summary>
		private void SaveSourceAs()
		{
			var dlg = new Microsoft.Win32.SaveFileDialog();

			// Set filter for file extension and default file extension
			var filter = "";

			foreach (BrainFuckDialect bfd in BFDialects.Dialects)
			{
				filter += bfd.Name + " documents (" + bfd.Extension + ")|*" + bfd.Extension + "|";
			}
			dlg.Filter = filter.Substring(0, filter.Length - 1);
			dlg.DefaultExt = BFDialects.Dialects[0].Extension;

			if ((bool)dlg.ShowDialog())
			{
				string[] parts = GetExtensionAndFileName(dlg.FileName);
				FileName = parts[0];
				SetDialectByExtension(parts[1]);
				TranslateDialect();
				SaveSource();
				Changed = false;
			}
		}

		/// <summary>
		/// Save logic.
		/// </summary>
		private void SaveSource()
		{
			// Save document
			var lines = new List<string>();
			foreach (Paragraph p in rtfBox.Document.Blocks.Cast<Paragraph>())
			{
				string line = "";
				foreach (Run r in p.Inlines.Cast<Run>())
				{
					if (r.Text != null)
					{
						line += r.Text;
					}
				}
				lines.Add(line);
			}
			File.WriteAllLines(FileName + BFDialect.Extension, lines.ToArray());
			Changed = false;
		}

		/// <summary>
		/// Decision if save needed and which is to be performed.
		/// </summary>
		private void CheckAndSave()
		{
			if (Changed)
			{
				if (FileName == null)
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
			changeEventsPermited = true;
			beautifyTimer.Stop();
			Beautify();
			changeEventsPermited = false;
			wmv.InitializeMemory();

			var source_pos = 0;
			var new_source_pos = 0;
			var jump = false;
			var jumpOver = false;
			var stack = new Stack<int>();
			var brackets = new Dictionary<int, int>();

			stdoutView.STDOUT = "";

			// Refresh source code.
			sourceCode =
			(
				from Paragraph p in rtfBox.Document.Blocks.Cast<Paragraph>()
				from Run r in p.Inlines.Cast<Run>()
				where r.Name.Equals("Command")
				select r
			).ToList();
			#endregion

			#region [ Fill brackets dictionary ]
			for (var i = 0; i < sourceCode.Count; i++)
			{
				var pe = (ProgrammElement)sourceCode[i].Tag;
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
				var unclosedBrackets = new List<int>();

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
				stdoutView.STDOUT += Environment.NewLine;
				stdoutView.STDOUT += string.Format("Brackets are inconsistent! End!");
				return;
			}

			stdoutView.STDOUT += Environment.NewLine;
			#endregion

			#region [ MAIN LOOP ]
			while (true)
			{
				changeEventsPermited = false;
				jump = false;
				jumpOver = false;

				var pe = (ProgrammElement)sourceCode[source_pos].Tag;
				sourceCode[source_pos].BringIntoView();

				if (pe.Breakpoint)
				{
					changeEventsPermited = false;
					sourceCode[source_pos].BringIntoView();
					sourceCode[source_pos].Background = Brushes.Tomato;

					var rs = new RunStep("Visual DingsBums - Breakpoint")
					{
						Owner = this,
						Hint = "Breakpoint: " + pe.Command.ToString()
					};
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
					sourceCode[source_pos].Background = Brushes.DarkOrange;
				}
				else if (mode)
				{
					changeEventsPermited = false;
					sourceCode[source_pos].Background = Brushes.DarkGray;
					sourceCode[source_pos].BringIntoView();

					var rs = new RunStep("Visual DingsBums - Run Step Mode")
					{
						Hint = "Next step: " + pe.Command.ToString(),
						Owner = this
					};
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
							var stdin = new STDINView
							{
								Owner = this
							};
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

			changeEventsPermited = true;
			beautifyTimer.Start();
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
				var currentLineNumber = 0;
				var caretPos = rtfBox.CaretPosition;
				var caretLineStart = rtfBox.CaretPosition.GetLineStartPosition(0);
				currentOffset = rtfBox.Document.ContentStart.GetOffsetToPosition(rtfBox.CaretPosition);

				foreach (Paragraph p in rtfBox.Document.Blocks.Cast<Paragraph>())
				{
					currentLineNumber++;
					if (p.Equals(caretLineStart.Paragraph))
					{
						break;
					}
				}

				var currentColumnNumber = Math.Max(caretLineStart.GetOffsetToPosition(caretPos) - 1, 0);
				TextBlockStatus.Text = string.Format("Line: {0}, Column: {1}, Offset: {2}", currentLineNumber, currentColumnNumber, currentOffset);
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
				var color = ((SolidColorBrush)brush).Color;
				var channel = new byte[4];
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
			changeEventsPermited = false;
			Beutifying = true;
			#endregion

			#region [ Fields ]
			Run cR;
			var newDocument = new FlowDocument();
			var commandCounter = 0;
			var lastOffset = currentOffset;
			#endregion

			#region [ Document recreation ]
			var LineCounter = 0;
			foreach (Paragraph p in rtfBox.Document.Blocks.Cast<Paragraph>())
			{
				#region [ Init loop run ]
				var newParagraph = new Paragraph
				{
					Tag = ++LineCounter
				};

				var breakpoints = new List<int>();
				#endregion

				#region [ Collect paragraph contents ]
				var workText = "";

				foreach (var i in p.Inlines)
				{
					cR = (Run)i;
					if (cR.Tag != null)
					{
						var pe = (ProgrammElement)cR.Tag;
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

					foreach (var bfli in BFDialect.DialectItems)
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
							var newRun = new Run
							{
								Text = bfli.DialectText,
								FontStyle = bfli.FontStyle,
								Foreground = bfli.Foreground,
								FontWeight = bfli.FontWeight,
								Name = "Command"
							};

							#region [ Create fancy tooltip ]
							var border = new Border
							{
								BorderBrush = Brushes.Black,
								Padding = new Thickness(10),
								Margin = new Thickness(0),
								BorderThickness = new Thickness(2),
								CornerRadius = new CornerRadius(5),
								Background = Brushes.White
							};

							var grd = new Grid();
							grd.ColumnDefinitions.Add(new ColumnDefinition());
							grd.ColumnDefinitions.Add(new ColumnDefinition());
							grd.ColumnDefinitions.Add(new ColumnDefinition());
							grd.ColumnDefinitions[0].Width = GridLength.Auto;
							grd.ColumnDefinitions[1].Width = new GridLength(20);
							grd.ColumnDefinitions[2].Width = GridLength.Auto;
							var img = new Image
							{
								Source = bfli.Picture,
								Height = 16,
								Width = 16
							};
							Grid.SetColumn(img, 0);
							grd.Children.Add(img);
							var txt = new TextBlock();
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
					foreach (var i in newParagraph.Inlines)
					{
						cR = (Run)i;
						if (cR.Tag != null)
						{
							var pe = (ProgrammElement)cR.Tag;

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
				rtfBox.Document = newDocument;
			}

			rtfBox.IsDocumentEnabled = true;
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

			var tp = rtfBox.Document.ContentStart;
			var np = tp.GetNextInsertionPosition(LogicalDirection.Forward);

			if (np != null)
			{
				var co = tp.GetOffsetToPosition(np);
				while (co < lastOffset)
				{
					var next = np.GetNextInsertionPosition(LogicalDirection.Forward);
					if (next == null)
					{
						break;
					}
					co = tp.GetOffsetToPosition(next);
					np = next;
				}
				rtfBox.Selection.Select(np, np);
			}
			#endregion

			#region [ Restore application state ]
			Beutifying = false;
			Beutified = true;
			changeEventsPermited = true;
			#endregion
		}

		/// <summary>
		/// Creates new comment chunk from string.
		/// </summary>
		/// <param name="Comment">string with comment content</param>
		/// <returns>Run with respective formattings</returns>
		private Run CloseComment(string Comment)
		{
			var newRun = new Run
			{
				Text = Comment,
				Foreground = Brushes.LightGray,
				FontStyle = FontStyles.Italic,
				Name = "Comment"
			};
			return newRun;
		}

		/// <summary>
		/// Switches break point on / off
		/// </summary>
		private void ToggleBreakpoint()
		{
			//TextPointer caretLineStart = this.rtfBox.CaretPosition.GetLineStartPosition(0);
			var caretLineStart = rtfBox.CaretPosition;

			if (caretLineStart.Parent is Run)
			{
				var currentRun = (Run)caretLineStart.Parent;
				if (currentRun.Name.Equals("Command"))
				{
					var pe = (ProgrammElement)currentRun.Tag;
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

			var bfd = new BrainFuckDialect("Marsupilamian", ".marsu", "Houba!", "Houbaaa!", "Houbi!", "Houbibi!", "Houba?", "Hop!", "Marsu?", "Marsu!");
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
			settingsDS = new DataSet();
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

			sz.Rows.Add(0, AppName, Left, Top, Width, Height);
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
