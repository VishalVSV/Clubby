using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.Discord
{
    public class Suggestion
    {
        public string suggestion;
        public string Author = "";
        public string icon_url = "";
        public int suggestion_number;

        public ulong msg_id;

        public int score;
    }
}
