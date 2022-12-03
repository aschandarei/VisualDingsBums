 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VisualDingsBums
{
    /// <summary>
    /// Represents single bf element with properties.
    /// </summary>
    public class BrainFuckLanguageItem
    {
        #region [ Properties ]
        /// <summary>
        /// BF Command associated with the language element.
        /// </summary>
        public BrainFuckCommand Command { get; private set; }

        /// <summary>
        /// New text representing the command.
        /// </summary>
        public string DialectText { get; private set; }

        /// <summary>
        /// Font style for the syntax highlighting.
        /// </summary>
        public FontStyle FontStyle { get; set; }

        /// <summary>
        /// Font weight for the syntax highlighting.
        /// </summary>
        public FontWeight FontWeight { get; set; }

        /// <summary>
        /// Font color for the syntax highlighting.
        /// </summary>
        public Brush Foreground { get; set; }

        /// <summary>
        /// Picture for the dialect browser.
        /// </summary>
        public ImageSource Picture { get; private set; }
        #endregion

        #region [ Constructor ]
        /// <summary>
        /// Language item constructor for new bf dialects.
        /// </summary>
        /// <param name="Command">bf command</param>
        /// <param name="DialectText">New text representing the command</param>
        public BrainFuckLanguageItem(BrainFuckCommand Command, string DialectText)
        {
            this.Command = Command;
            this.DialectText = DialectText;
            this.FontStyle = FontStyles.Normal;
            this.Foreground = Brushes.Blue;
            this.FontWeight = FontWeights.Normal;
            SetPicture();
        }

        /// <summary>
        /// Language item empty constructor. Returns 'Classic' bf element
        /// </summary>
        /// <param name="Command">bf command</param>
        /// <param name="DialectText">New text representing the command</param>
        public BrainFuckLanguageItem(BrainFuckCommand Command)
        {
            this.Command = Command;
            this.DialectText = GetOriginalBFCommandText(Command);
            this.FontStyle = FontStyles.Normal;
            this.Foreground = Brushes.Blue;
            this.FontWeight = FontWeights.Normal;
            SetPicture();
        } 
        #endregion

        #region [ Methods ]
        /// <summary>
        /// Returns the original BF command text.
        /// </summary>
        /// <param name="Command">BF command</param>
        /// <returns>string with the original BF command text</returns>
        private string GetOriginalBFCommandText(BrainFuckCommand Command)
        {
            // Set original BF command text.
            switch (Command)
            {
                case BrainFuckCommand.Decrement_cell:
                    {
                        return "-";
                    }
                case BrainFuckCommand.Increment_cell:
                    {
                        return "+";
                    }
                case BrainFuckCommand.Move_to_previous_cell:
                    {
                        return "<";
                    }
                case BrainFuckCommand.Move_to_next_cell:
                    {
                        return ">";
                    }
                case BrainFuckCommand.Loop_Tail_Condition:
                    {
                        return "]";
                    }
                case BrainFuckCommand.Loop_Head_Condition:
                    {
                        return "[";
                    }
                case BrainFuckCommand.Input_cell:
                    {
                        return ",";
                    }
                case BrainFuckCommand.Output_cell:
                    {
                        return ".";
                    }
            }
            return "";
        }

        /// <summary>
        /// Sets the picture according to the command.
        /// </summary>
        private void SetPicture()
        {
            switch (this.Command)
            {
                case BrainFuckCommand.Decrement_cell:
                    {
                        this.Picture = new BitmapImage(new Uri("../IMAGES/Decrement.png", UriKind.Relative));
                        break;
                    }
                case BrainFuckCommand.Increment_cell:
                    {
                        this.Picture = new BitmapImage(new Uri("../IMAGES/Increment.png", UriKind.Relative));
                        break;
                    }
                case BrainFuckCommand.Move_to_next_cell:
                    {
                        this.Picture = new BitmapImage(new Uri("../IMAGES/Next.png", UriKind.Relative));
                        break;
                    }
                case BrainFuckCommand.Move_to_previous_cell:
                    {
                        this.Picture = new BitmapImage(new Uri("../IMAGES/Previous.png", UriKind.Relative));
                        break;
                    }
                case BrainFuckCommand.Input_cell:
                    {
                        this.Picture = new BitmapImage(new Uri("../IMAGES/Input.png", UriKind.Relative));
                        break;
                    }
                case BrainFuckCommand.Output_cell:
                    {
                        this.Picture = new BitmapImage(new Uri("../IMAGES/Output.png", UriKind.Relative));
                        break;
                    }
                case BrainFuckCommand.Loop_Head_Condition:
                    {
                        this.Picture = new BitmapImage(new Uri("../IMAGES/Up.png", UriKind.Relative));
                        break;
                    }
                case BrainFuckCommand.Loop_Tail_Condition:
                    {
                        this.Picture = new BitmapImage(new Uri("../IMAGES/Down.png", UriKind.Relative));
                        break;
                    }
            }
        }
        #endregion
    }
}
