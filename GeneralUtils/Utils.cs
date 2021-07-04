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

        public static void AddOrUpdate<T,U>(this Dictionary<T,U> dict, T key, U value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
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

        static string keyboard = "`~1!2@3#4$5%6^7&8*9(0)-_=+\nQqWwEeRrTtYyUuIiOoPp{[}]|\\\nAaSsDdFfGgHhJjKkLl:;\"'\nZzXxCcVvBbNnMm<,>.?/";

        static int GetKeyDist(char a, char b)
        {
            var (ax, ay) = GetKeyPos(a);
            var (bx, by) = GetKeyPos(b);
            return Math.Max(Math.Abs(ax - bx), Math.Abs(ay - by));
        }

        static (int, int) GetKeyPos(char a)
        {
            int x = 0, y = 0;

            for (int i = 0; i < keyboard.Length; i++)
            {
                if (keyboard[i] == a)
                {
                    break;
                }
                else
                {
                    if (keyboard[i] == '\n')
                    {
                        y += 1;
                        x = 0;
                    }
                    else
                    {
                        x += 1;
                    }
                }
            }

            return (x / 2, y);
        }

        public static int LevenshteinDistanceModified(string s, string t)
        {
            const int insertion_cost = 2, deletion_cost = 2;

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m * insertion_cost;
            }

            if (m == 0)
            {
                return n * insertion_cost;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : GetKeyDist(t[j - 1], s[i - 1]);

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + deletion_cost,
                        d[i, j - 1] + insertion_cost),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public static string GetBestMatch(this List<string> ls, string match, int threshold = 2)
        {
            for (int i = 0; i < ls.Count; i++)
            {
                int d = LevenshteinDistanceModified(ls[i], match);
                if (d <= threshold)
                {
                    return ls[i];
                }
            }

            return null;
        }
    }
}
