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

        // A nicety to have when you need to do things in a chain. There is probably a LINQ implementation but I like this.
        public static U Then<T, U>(this T t, Func<T, U> func)
        {
            return func(t);
        }

        // Then but with null checking and defaults
        public static U ThenOrElse<T, U>(this T t, Func<T, U> func, U else_val)
        {
            if (t != null)
                return func(t);
            else
                return else_val;
        }

        public static T IfNull<T>(this T t, Func<T> func)
        {
            if (t == null) return func();
            else return t;
        }

        // Helper function to add to a list or create one if it doesn't exist
        public static void AddOrAppend<T, U>(this Dictionary<T, List<U>> dict, T key, U val)
        {
            if (dict.ContainsKey(key))
            {
                dict[key].Add(val);
            }
            else
            {
                dict.Add(key, new List<U>() { val });
            }
        }

        // The same as above but checks for duplicates
        public static void AddOrAppendUnique<T, U>(this Dictionary<T, List<U>> dict, T key, U val)
        {
            if (dict.ContainsKey(key))
            {
                if (!dict[key].Contains(val))
                    dict[key].Add(val);
            }
            else
            {
                dict.Add(key, new List<U>() { val });
            }
        }

        // My prefered time formatting because format strings are too hard to understand.
        // If someone is cringing at this, I'm truly sorry about this.
        public static string ToPrettyString(this TimeSpan span)
        {
            return $"{(span.Days == 0 ? "" : $"{span.Days} day{(span.Days > 1 ? "s" : "")} ")}{(span.Hours == 0 ? "" : $"{span.Hours} hr{(span.Hours > 1 ? "s" : "")}")} {(span.Minutes == 0 ? "" : $"{span.Minutes} min{(span.Minutes > 1 ? "s" : "")}")} {(span.Seconds == 0 ? "" : $"{span.Seconds} sec{(span.Seconds > 1 ? "s" : "")}")}";
        }

        public static string MakeString<T>(this IEnumerable<T> a)
        {
            StringBuilder res = new StringBuilder();

            res.Append("[");
            foreach (var item in a)
            {
                res.Append(item);
                res.Append(", ");
            }
            res.Remove(res.Length - 2, 2);
            res.Append("]");

            return res.ToString();
        }
    }
}
