using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.GeneralUtils
{
    /// <summary>
    /// Static Logger to do basic log tracking
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Log a message inside a instance class
        /// </summary>
        /// <param name="sender">The instance of the class logging the message</param>
        /// <param name="message">The message to be logged</param>
        public static void Log(object sender,string message)
        {
            Console.WriteLine($"{sender.GetType().Name}:  {message}");
        }

        /// <summary>
        /// Log a message inside a static class
        /// </summary>
        /// <param name="message">The message to be logged</param>
        /// <typeparam name="T">The type of the class logging the message</typeparam>
        public static void Log<T>(string message)
        {
            Console.WriteLine($"{typeof(T).Name}:  {message}");
        }
    }
}
