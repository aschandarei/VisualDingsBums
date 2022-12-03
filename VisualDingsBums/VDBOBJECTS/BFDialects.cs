using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDingsBums
{
    /// <summary>
    /// Provides global collection of BF dialects.
    /// </summary>
    public static class BFDialects
    {
        public static ObservableCollection<BrainFuckDialect> Dialects { get; set; }
    }
}
