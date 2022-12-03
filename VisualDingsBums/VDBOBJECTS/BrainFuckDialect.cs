using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace VisualDingsBums
{
    /// <summary>
    /// Creates new BF commands dialect or 'clasic' bf commands set.
    /// </summary>
    public class BrainFuckDialect: INotifyPropertyChanged
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
        #region [ Dialect items ]
        private ReadOnlyCollection<BrainFuckLanguageItem> _dialectItems;

        /// <summary>
        /// Read-Only bf command set.
        /// </summary>
        public ReadOnlyCollection<BrainFuckLanguageItem> DialectItems
        {
            get
            {
                return this._dialectItems;
            }
        } 
        #endregion

        #region [ Name ]
        private string _name;

        /// <summary>
        /// Name of the BF dialect.
        /// </summary>
        public string Name
        {
            get
            {
                return this._name;
            }

            private set
            {
                this._name = value;
                OnPropertyChanged("Name");
            }
        }
        #endregion

        #region [ Extension ]
        private string _extension;

        /// <summary>
        /// File extension for files with this BF dialect.
        /// </summary>
        public string Extension
        {
            get
            {
                return this._extension;
            }
            
            private set
            {
                this._extension = value;
                OnPropertyChanged("Extension");
            }
        }
        #endregion
        #endregion

        #region [ Constructor ]
        /// <summary>
        /// Constructor for creating new bf dialects.
        /// </summary>
        /// <param name="Name">Dialect Name</param>
        /// <param name="Name">File Extension</param>
        /// <param name="Move_to_previous_cell_DialectText">Text to use instead of classic 'less than' char</param>
        /// <param name="Move_to_next_cell_DialectText">Text to use instead of classic 'greater than' char</param>
        /// <param name="Increment_cell_DialectText">Text to use instead of classic '+' char</param>
        /// <param name="Decrement_cell_DialectText">Text to use instead of classic '-' sign</param>
        /// <param name="Loop_Head_Condition_DialectText">Text to use instead of classic '[' char</param>
        /// <param name="Loop_Tail_Condition_DialectText">Text to use instead of classic ']' char</param>
        /// <param name="Input_cell_DialectText">Text to use instead of classic ',' char</param>
        /// <param name="Output_cell_DialectText">Text to use instead of classic '.' char</param>
        public BrainFuckDialect
            (
            string Name,
            string Extension,
            string Move_to_previous_cell_DialectText,
            string Move_to_next_cell_DialectText,
            string Increment_cell_DialectText,
            string Decrement_cell_DialectText,
            string Loop_Head_Condition_DialectText,
            string Loop_Tail_Condition_DialectText,
            string Input_cell_DialectText,
            string Output_cell_DialectText
            )
        {
            this.Name = Name;
            this.Extension = Extension;

            List<BrainFuckLanguageItem> bfel = new List<BrainFuckLanguageItem>();
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Move_to_previous_cell, Move_to_previous_cell_DialectText));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Move_to_next_cell, Move_to_next_cell_DialectText));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Increment_cell, Increment_cell_DialectText));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Decrement_cell, Decrement_cell_DialectText));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Loop_Head_Condition, Loop_Head_Condition_DialectText));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Loop_Tail_Condition, Loop_Tail_Condition_DialectText));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Input_cell, Input_cell_DialectText));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Output_cell, Output_cell_DialectText));
            this._dialectItems = new ReadOnlyCollection<BrainFuckLanguageItem>(bfel);
        }

        /// <summary>
        /// Standart constructor - returns 'clasic' bf set.
        /// </summary>
        public BrainFuckDialect()
        {
            this.Name = "Brain F*ck";
            this.Extension = ".bf";
            List<BrainFuckLanguageItem> bfel = new List<BrainFuckLanguageItem>();
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Move_to_previous_cell));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Move_to_next_cell));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Increment_cell));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Decrement_cell));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Loop_Head_Condition));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Loop_Tail_Condition));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Input_cell));
            bfel.Add(new BrainFuckLanguageItem(BrainFuckCommand.Output_cell));
            this._dialectItems = new ReadOnlyCollection<BrainFuckLanguageItem>(bfel);
        }

        /// <summary>
        /// Re-Create BF dialect constructor from given DataRow.
        /// </summary>
        /// <param name="row">DataRow containig BF dialect data.</param>
        public BrainFuckDialect(DataRow row)
        {
            this.Name = row["DialectName"].ToString();
            this.Extension = row["FileExtension"].ToString();
            List<BrainFuckLanguageItem> bfel = new List<BrainFuckLanguageItem>();

            for (byte i = 0; i < 8; i++)
            {
                BrainFuckCommand bfc = (BrainFuckCommand)Enum.ToObject(typeof(BrainFuckCommand), i);
                BrainFuckLanguageItem bfl = new BrainFuckLanguageItem(bfc, row[bfc.ToString()].ToString());
                bfl.Foreground = BrushFromColor(Convert.ToInt32(row[bfc.ToString() + "_Foreground"]));

                if (row[bfc.ToString() + "_Fontstyle"].ToString().Equals("Italic"))
                {
                    bfl.FontStyle = FontStyles.Italic;
                }
                else if (row[bfc.ToString() + "_Fontstyle"].ToString().Equals("Oblique"))
                {
                    bfl.FontStyle = FontStyles.Oblique;
                }

                if (row[bfc.ToString() + "_Fontweight"].ToString().Equals("Bold"))
                {
                    bfl.FontWeight = FontWeights.Bold;
                }
                else if (row[bfc.ToString() + "_Fontweight"].ToString().Equals("Medium"))
                {
                    bfl.FontWeight = FontWeights.Medium;
                }

                bfel.Add(bfl);
            }
            this._dialectItems = new ReadOnlyCollection<BrainFuckLanguageItem>(bfel);
        }
        #endregion

        #region [ Methods ]
        /// <summary>
        /// Converts given 32 bit integer to solid color brush.
        /// </summary>
        /// <param name="intColor">32 Bit integer valu to be converted.</param>
        /// <returns></returns>
        private Brush BrushFromColor(int intColor)
        {
            byte[] channel = BitConverter.GetBytes(intColor);
            Color color = Color.FromArgb(channel[0], channel[1], channel[2], channel[3]);
            return new SolidColorBrush(color);
        } 

        /// <summary>
        /// Change the name.
        /// </summary>
        /// <param name="NewName">New name for the dialect.</param>
        public void ChangeName(string NewName)
        {
            this.Name = NewName;
        }

        /// <summary>
        /// Change the file extension.
        /// </summary>
        /// <param name="NewName">New file extension for the dialect.</param>
        public void ChangeExtension(string NewExtension)
        {
            this.Extension = NewExtension;
        }

        /// <summary>
        /// Changes the text for given command.
        /// </summary>
        /// <param name="Command">Command to change the dialext text for.</param>
        /// <param name="DialectText">New dialect text for the command.</param>
        public void ChangeDialectText(BrainFuckCommand Command, string DialectText)
        {
            List<BrainFuckLanguageItem> bfel = new List<BrainFuckLanguageItem>();
            foreach(BrainFuckLanguageItem bfli in this._dialectItems)
            {
                if (bfli.Command == Command)
                {
                    BrainFuckLanguageItem item = new BrainFuckLanguageItem(Command, DialectText);
                    bfel.Add(item);
                }
                else
                {
                    bfel.Add(bfli);
                }
            }
            this._dialectItems = new ReadOnlyCollection<BrainFuckLanguageItem>(bfel);
            OnPropertyChanged("DialectItems");
        }

        /// <summary>
        /// Changes the font style for given command.
        /// </summary>
        /// <param name="Command">Command to change the dialext text for.</param>
        /// <param name="DialectText">New font style for the command.</param>
        public void ChangeFontStyle(BrainFuckCommand Command, FontStyle fontStyle)
        {
            List<BrainFuckLanguageItem> bfel = new List<BrainFuckLanguageItem>();
            foreach (BrainFuckLanguageItem bfli in this._dialectItems)
            {
                if (bfli.Command == Command)
                {
                    bfli.FontStyle = fontStyle;
                }

                bfel.Add(bfli);
            }
            this._dialectItems = new ReadOnlyCollection<BrainFuckLanguageItem>(bfel);
            OnPropertyChanged("DialectItems");
        }

        /// <summary>
        /// Changes the font weight for given command.
        /// </summary>
        /// <param name="Command">Command to change the dialext text for.</param>
        /// <param name="DialectText">New dialct text for the command.</param>
        public void ChangeFontWeight(BrainFuckCommand Command, FontWeight fontWeight)
        {
            List<BrainFuckLanguageItem> bfel = new List<BrainFuckLanguageItem>();
            foreach (BrainFuckLanguageItem bfli in this._dialectItems)
            {
                if (bfli.Command == Command)
                {
                    bfli.FontWeight = fontWeight;
                }

                bfel.Add(bfli);
            }
            this._dialectItems = new ReadOnlyCollection<BrainFuckLanguageItem>(bfel);
            OnPropertyChanged("DialectItems");
        }

        /// <summary>
        /// Changes the font color for given command.
        /// </summary>
        /// <param name="Command">Command to change the color for.</param>
        /// <param name="DialectText">New dialct text for the command.</param>
        public void ChangeColor(BrainFuckCommand Command, Brush Color)
        {
            List<BrainFuckLanguageItem> bfel = new List<BrainFuckLanguageItem>();
            foreach (BrainFuckLanguageItem bfli in this._dialectItems)
            {
                if (bfli.Command == Command)
                {
                    bfli.Foreground = Color;
                }

                bfel.Add(bfli);
            }
            this._dialectItems = new ReadOnlyCollection<BrainFuckLanguageItem>(bfel);
            OnPropertyChanged("DialectItems");
        }
        #endregion
    }
}
