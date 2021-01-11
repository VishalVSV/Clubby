using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.GeneralUtils
{
    /// <summary>
    /// General language niceties that I like to keep
    /// </summary>
    public static class Utils
    {
        public delegate void Initializer<T>(ref T t);

        /// <summary>
        /// Initialize a struct using a closure
        /// </summary>
        /// <typeparam name="T">Type of the struct to initialize</typeparam>
        /// <param name="initializer">The initializer closure</param>
        public static T Init<T>(Initializer<T> initializer)
            where T : struct
        {
            T t = new T();
            initializer(ref t);
            return t;
        }
    }
}
