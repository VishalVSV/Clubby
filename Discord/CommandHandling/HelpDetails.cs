namespace Clubby.Discord.CommandHandling
{
    /// <summary>
    /// The struct that contains discord command metadata.
    /// </summary>
    public struct HelpDetails
    {
        /// <summary>
        /// The name of the command to be displayed
        /// </summary>
        public string CommandName;
        /// <summary>
        /// A short description of the command to display on help.
        /// </summary>
        public string ShortDescription;
        
        /* Example HelpDetails
            public static HelpDetails Example = Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "example";
                h.ShortDescription = "Example command";
            });
        */
    }
}
