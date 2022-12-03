using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDingsBums
{
    /// <summary>
    /// Provides enumeration for the 8 available bf commands.
    /// </summary>
    public enum BrainFuckCommand : int
    {
        /// <summary>
        /// Represents BF '<' command.
        /// </summary>
        Move_to_previous_cell = 0,
        /// <summary>
        /// Represents BF '>' command.
        /// </summary>
        Move_to_next_cell = 1,
        /// <summary>
        /// epresents BF '+' command.
        /// </summary>
        Increment_cell = 2,
        /// <summary>
        /// Represents BF '-' command.
        /// </summary>
        Decrement_cell = 3,
        /// <summary>
        /// Represents BF '[' command.
        /// </summary>
        Loop_Head_Condition = 4,
        /// <summary>
        /// Represents BF ']' command.
        /// </summary>
        Loop_Tail_Condition = 5,
        /// <summary>
        /// Represents BF ',' command.
        /// </summary>
        Input_cell = 6,
        /// <summary>
        /// Represents BF '.' command.
        /// </summary>
        Output_cell = 7
    }
}
