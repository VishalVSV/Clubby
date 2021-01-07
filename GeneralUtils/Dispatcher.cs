using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.GeneralUtils
{
    public static class Dispatcher
    {

        public static Dictionary<Type, Func<string, object>> converter = new Dictionary<Type, Func<string, object>>()
        {
            { typeof(int),(s) => int.Parse(s) },
            { typeof(double),(s) => double.Parse(s) },
            { typeof(string),(s) => s },
        };
        public static void Dispatch<T0>(Action<T0> action, string msg, params string[] error_msgs)
        {
            string[] segs = msg.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            if (segs.Length != 1) throw new Exception($"Expected 1 arguments got {segs.Length}");
            bool init = true;
            T0 t0 = default;
            if (converter.ContainsKey(typeof(T0)))
            {
                try { t0 = (T0)converter[typeof(T0)](segs[0].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 1 < error_msgs.Length) throw new Exception(error_msgs[1]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T0)}");
            if (!init) throw new Exception($"Couldn't convert {segs[0]} to {typeof(T0).Name}");
            action(t0);
        }

        public static void Dispatch<T0, T1>(Action<T0, T1> action, string msg, params string[] error_msgs)
        {
            string[] segs = msg.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            if (segs.Length != 2) throw new Exception($"Expected 2 arguments got {segs.Length}");
            bool init = true;
            T0 t0 = default;
            if (converter.ContainsKey(typeof(T0)))
            {
                try { t0 = (T0)converter[typeof(T0)](segs[0].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 2 < error_msgs.Length) throw new Exception(error_msgs[2]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T0)}");
            if (!init) throw new Exception($"Couldn't convert {segs[0]} to {typeof(T0).Name}");
            T1 t1 = default;
            if (converter.ContainsKey(typeof(T1)))
            {
                try { t1 = (T1)converter[typeof(T1)](segs[1].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 2 < error_msgs.Length) throw new Exception(error_msgs[2]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T1)}");
            if (!init) throw new Exception($"Couldn't convert {segs[1]} to {typeof(T1).Name}");
            action(t0, t1);
        }

        public static void Dispatch<T0, T1, T2>(Action<T0, T1, T2> action, string msg, params string[] error_msgs)
        {
            string[] segs = msg.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            if (segs.Length != 3) throw new Exception($"Expected 3 arguments got {segs.Length}");
            bool init = true;
            T0 t0 = default;
            if (converter.ContainsKey(typeof(T0)))
            {
                try { t0 = (T0)converter[typeof(T0)](segs[0].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 3 < error_msgs.Length) throw new Exception(error_msgs[3]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T0)}");
            if (!init) throw new Exception($"Couldn't convert {segs[0]} to {typeof(T0).Name}");
            T1 t1 = default;
            if (converter.ContainsKey(typeof(T1)))
            {
                try { t1 = (T1)converter[typeof(T1)](segs[1].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 3 < error_msgs.Length) throw new Exception(error_msgs[3]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T1)}");
            if (!init) throw new Exception($"Couldn't convert {segs[1]} to {typeof(T1).Name}");
            T2 t2 = default;
            if (converter.ContainsKey(typeof(T2)))
            {
                try { t2 = (T2)converter[typeof(T2)](segs[2].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 3 < error_msgs.Length) throw new Exception(error_msgs[3]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T2)}");
            if (!init) throw new Exception($"Couldn't convert {segs[2]} to {typeof(T2).Name}");
            action(t0, t1, t2);
        }

        public static void Dispatch<T0, T1, T2, T3>(Action<T0, T1, T2, T3> action, string msg, params string[] error_msgs)
        {
            string[] segs = msg.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            if (segs.Length != 4) throw new Exception($"Expected 4 arguments got {segs.Length}");
            bool init = true;
            T0 t0 = default;
            if (converter.ContainsKey(typeof(T0)))
            {
                try { t0 = (T0)converter[typeof(T0)](segs[0].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 4 < error_msgs.Length) throw new Exception(error_msgs[4]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T0)}");
            if (!init) throw new Exception($"Couldn't convert {segs[0]} to {typeof(T0).Name}");
            T1 t1 = default;
            if (converter.ContainsKey(typeof(T1)))
            {
                try { t1 = (T1)converter[typeof(T1)](segs[1].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 4 < error_msgs.Length) throw new Exception(error_msgs[4]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T1)}");
            if (!init) throw new Exception($"Couldn't convert {segs[1]} to {typeof(T1).Name}");
            T2 t2 = default;
            if (converter.ContainsKey(typeof(T2)))
            {
                try { t2 = (T2)converter[typeof(T2)](segs[2].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 4 < error_msgs.Length) throw new Exception(error_msgs[4]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T2)}");
            if (!init) throw new Exception($"Couldn't convert {segs[2]} to {typeof(T2).Name}");
            T3 t3 = default;
            if (converter.ContainsKey(typeof(T3)))
            {
                try { t3 = (T3)converter[typeof(T3)](segs[3].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 4 < error_msgs.Length) throw new Exception(error_msgs[4]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T3)}");
            if (!init) throw new Exception($"Couldn't convert {segs[3]} to {typeof(T3).Name}");
            action(t0, t1, t2, t3);
        }

        public static void Dispatch<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> action, string msg, params string[] error_msgs)
        {
            string[] segs = msg.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            if (segs.Length != 5) throw new Exception($"Expected 5 arguments got {segs.Length}");
            bool init = true;
            T0 t0 = default;
            if (converter.ContainsKey(typeof(T0)))
            {
                try { t0 = (T0)converter[typeof(T0)](segs[0].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 5 < error_msgs.Length) throw new Exception(error_msgs[5]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T0)}");
            if (!init) throw new Exception($"Couldn't convert {segs[0]} to {typeof(T0).Name}");
            T1 t1 = default;
            if (converter.ContainsKey(typeof(T1)))
            {
                try { t1 = (T1)converter[typeof(T1)](segs[1].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 5 < error_msgs.Length) throw new Exception(error_msgs[5]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T1)}");
            if (!init) throw new Exception($"Couldn't convert {segs[1]} to {typeof(T1).Name}");
            T2 t2 = default;
            if (converter.ContainsKey(typeof(T2)))
            {
                try { t2 = (T2)converter[typeof(T2)](segs[2].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 5 < error_msgs.Length) throw new Exception(error_msgs[5]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T2)}");
            if (!init) throw new Exception($"Couldn't convert {segs[2]} to {typeof(T2).Name}");
            T3 t3 = default;
            if (converter.ContainsKey(typeof(T3)))
            {
                try { t3 = (T3)converter[typeof(T3)](segs[3].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 5 < error_msgs.Length) throw new Exception(error_msgs[5]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T3)}");
            if (!init) throw new Exception($"Couldn't convert {segs[3]} to {typeof(T3).Name}");
            T4 t4 = default;
            if (converter.ContainsKey(typeof(T4)))
            {
                try { t4 = (T4)converter[typeof(T4)](segs[4].Trim()); }
                catch (Exception)
                {
                    if (error_msgs != null && 5 < error_msgs.Length) throw new Exception(error_msgs[5]);
                    else init = false;
                }
            }
            else throw new Exception($"Can't convert argument to {typeof(T4)}");
            if (!init) throw new Exception($"Couldn't convert {segs[4]} to {typeof(T4).Name}");
            action(t0, t1, t2, t3, t4);
        }
    }
}
