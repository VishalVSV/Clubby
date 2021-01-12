using System;

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

        public static string ToPrettyString(this TimeSpan span)
        {
            return $"{(span.Hours == 0 ? "" : $"{span.Hours} hr{(span.Hours > 1 ? "s" : "")}")} {(span.Minutes == 0 ? "" : $"{span.Minutes} min{(span.Minutes > 1 ? "s" : "")}")} {(span.Seconds == 0 ? "" : $"{span.Seconds} sec{(span.Seconds > 1 ? "s" : "")}")}";
        }
    }
}
