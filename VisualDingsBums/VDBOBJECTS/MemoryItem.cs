using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDingsBums
{
    /// <summary>
    /// Represents single memory item with content - DEC, HEX, BIN
    /// </summary>
    public class MemoryItem : INotifyPropertyChanged
    {
        #region [ INotifyPropertyChanged ]
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        } 
        #endregion

        #region [ Constructor ]
        /// <summary>
        /// Creates new memory item.
        /// </summary>
        /// <param name="ID">Id of the item.</param>
        /// <param name="Value">Value of the item.</param>
        public MemoryItem(int ID, byte Value)
        {
            this.Value = Value;
            this.ID = ID;
        }

        /// <summary>
        /// Creates new memory item with default (0) value.
        /// </summary>
        /// <param name="ID">Id of the item.</param>
        public MemoryItem(int ID)
        {
            this.Value = 0;
            this.ID = ID;
        } 
        #endregion

        #region [ Properties ]
        #region [ ID ]
        /// <summary>
        /// ID of the item.
        /// </summary>
        public int ID
        {
            get;
            set;
        } 
        #endregion

        #region [ Value ]
        private byte _value;
        /// <summary>
        /// Value(byte).
        /// </summary>
        public byte Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                RaisePropertyChanged("Value");
                RaisePropertyChanged("Char");
                RaisePropertyChanged("BIN");
                RaisePropertyChanged("HEX");
            }
        } 
        #endregion

        #region [ Value as char ]
        /// <summary>
        /// Read-only value as char
        /// </summary>
        public char Char
        {
            get
            {
                return (char)Value;
            }
        } 
        #endregion

        #region [ Value as HEX string ]
        /// <summary>
        /// Read-only value as HEX string
        /// </summary>
        public string HEX
        {
            get
            {
                return Value.ToString("X");
            }
        }
        #endregion

        #region [ Value as BIN string ]
        /// <summary>
        /// Read-only value as BIN string.
        /// </summary>
        public string BIN
        {
            get
            {
                return Convert.ToString(Value, 2).PadLeft(8, '0');
            }
        } 
        #endregion
        #endregion
    }
}
