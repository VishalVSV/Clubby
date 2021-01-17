using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.Club
{
    /// <summary>
    /// Instance of a debate. Used to update register automatically.
    /// </summary>
    public class Debate
    {
        /// <summary>
        /// The date and time of the debate
        /// </summary>
        public DateTime date;
        /// <summary>
        /// The proposition team of the debate
        /// </summary>
        public string Proposition = null;
        /// <summary>
        /// The opposition team of the debate
        /// </summary>
        public string Opposition = null;
        /// <summary>
        /// The context for the debate
        /// </summary>
        public string Context = null;
        /// <summary>
        /// The motion of the debate
        /// </summary>
        public string Motion = null;
        /// <summary>
        /// Any remarks for the debate
        /// </summary>
        public string Remarks = null;
        /// <summary>
        /// The people who judged the debate
        /// </summary>
        public string Judges = null;
    }
}
