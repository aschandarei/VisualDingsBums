using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDingsBums
{
    /// <summary>
    /// Provides code element for running the programm.
    /// </summary>
    public class ProgrammElement
    {
        public BrainFuckCommand Command { get; private set; }
        public int Position { get; private set; }
        public bool Breakpoint { get; set; }

        public ProgrammElement(BrainFuckCommand command, int position, bool breakpoint)
        {
            this.Position = position;
            this.Command = command;
            this.Breakpoint = breakpoint;
        }
    }
}
