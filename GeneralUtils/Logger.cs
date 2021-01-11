using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.GeneralUtils
{
    public static class Logger
    {
        public static void Log(object sender,string message)
        {
            Console.WriteLine($"{sender.GetType().Name}:  {message}");
        }

        public static void Log<T>(string message)
        {
            Console.WriteLine($"{typeof(T).Name}:  {message}");
        }
    }
}
