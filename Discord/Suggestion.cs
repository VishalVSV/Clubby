using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.Discord
{
    /// <summary>
    /// Data class for Discord suggestions
    /// </summary>
    public class Suggestion
    {
        /// <summary>
        /// The actual suggestion
        /// </summary>
        public string suggestion;
        /// <summary>
        /// The username of the person who made the suggestion
        /// </summary>
        public string Author = "";
        /// <summary>
        /// The icon of the suggestor for embed purposes
        /// </summary>
        public string icon_url = "";
        /// <summary>
        /// The suggestion number to use to find it in the data and to decorate the embed.
        /// </summary>
        public int suggestion_number;

        /// <summary>
        /// The id of the suggestion message to update embed colors.
        /// </summary>
        public ulong msg_id;

        /// <summary>
        /// The current score of the suggestion
        /// </summary>
        public int score;
    }
}
