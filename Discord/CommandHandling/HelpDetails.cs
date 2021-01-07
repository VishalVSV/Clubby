using Clubby.GeneralUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.Discord.CommandHandling
{
    public struct HelpDetails
    {
        public string CommandName;
        public string ShortDescription;

        public (string, string)[] Parameters;//Name of the parameter followed by expected type of parameter

        public static HelpDetails Example = Utils.Init((ref HelpDetails h) =>
        {
            h.CommandName = "example";
            h.ShortDescription = "Example command";
            h.Parameters = new (string, string)[]
            {
                ("test argument","Should be a date")
            };
        });
    }
}
