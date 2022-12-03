using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualDingsBums
{
    public static class WPFCommands
    {
        public static readonly RoutedUICommand RunFull = new RoutedUICommand("RunFull", "RunFull", typeof(WPFCommands), new InputGestureCollection() { new KeyGesture(Key.F5, ModifierKeys.None) });
        public static readonly RoutedUICommand RunStep = new RoutedUICommand("RunStep", "RunStep", typeof(WPFCommands), new InputGestureCollection() { new KeyGesture(Key.F6, ModifierKeys.None) });
        public static readonly RoutedUICommand ToggleBreakpoint = new RoutedUICommand("ToggleBreakpoint", "ToggleBreakpoint", typeof(WPFCommands), new InputGestureCollection() { new KeyGesture(Key.F9, ModifierKeys.None) });
        public static readonly RoutedUICommand ClearAllBreakpoints = new RoutedUICommand("ClearAllBreakpoints", "ClearAllBreakpoints", typeof(WPFCommands));
        public static readonly RoutedUICommand FileOpen = new RoutedUICommand("FileOpen", "FileOpen", typeof(WPFCommands), new InputGestureCollection() { new KeyGesture(Key.O, ModifierKeys.Control) });
        public static readonly RoutedUICommand FileSave = new RoutedUICommand("FileSave", "FileSave", typeof(WPFCommands), new InputGestureCollection() { new KeyGesture(Key.S, ModifierKeys.Control) });
        public static readonly RoutedUICommand FileSaveAs = new RoutedUICommand("FileSaveAs", "FileSaveAs", typeof(WPFCommands));
        public static readonly RoutedUICommand FileNew = new RoutedUICommand("FileNew", "FileNew", typeof(WPFCommands), new InputGestureCollection() { new KeyGesture(Key.N, ModifierKeys.Control) });
        public static readonly RoutedUICommand AppQuit = new RoutedUICommand("AppQuit", "AppQuit", typeof(WPFCommands), new InputGestureCollection() { new KeyGesture(Key.Q, ModifierKeys.Control) });
        public static readonly RoutedUICommand ChangeDialectCommand = new RoutedUICommand("ChangeDialectCommand", "ChangeDialectCommand", typeof(WPFCommands), new InputGestureCollection() { new KeyGesture(Key.D, ModifierKeys.Control) });
    }
}
