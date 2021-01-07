using Clubby.ConfigService;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Clubby.Events
{
    public static class BetterEventHandler
    {
        public static ConfigFile config = null;

        private static Queue<(string, object[])> EventQueue = new Queue<(string, object[])>();
        private static List<Event> SortedEventCache = new List<Event>();

        static BetterEventHandler()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            for (int l = 0; l < types.Length; l++)
            {
                MethodInfo[] methods = types[l].GetMethods(BindingFlags.Public | BindingFlags.Static);

                for (int i = 0; i < methods.Length; i++)
                {
                    EventData eventData;
                    if ((eventData = methods[i].GetCustomAttribute<EventData>()) != null)
                    {
                        Event new_event = new Event();
                        new_event.EventId = eventData.EventId;

                        new_event.Constraints = new (ConstraintType, string[])[eventData.Constraints.Length];
                        for (int o = 0; o < eventData.Constraints.Length; o++)
                        {
                            if (eventData.Constraints[o].Item1 == ConstraintType.Eq || eventData.Constraints[o].Item1 == ConstraintType.NEq)
                            {
                                new_event.Constraints[o] = (eventData.Constraints[o].Item1, eventData.Constraints[o].Item2.Split(',').Select(s => s.Trim()).ToArray());
                            }
                            else
                            {
                                new_event.Constraints[o] = (eventData.Constraints[o].Item1, new string[] { eventData.Constraints[o].Item2 });
                            }
                        }

                        new_event.method = methods[i];
                        if (SortedEventCache.Count == 0)
                            SortedEventCache.Add(new_event);
                        else
                        {
                            bool inserted = false;
                            for (int k = 0; k < SortedEventCache.Count; k++)
                            {
                                if (SortedEventCache[k].Constraints.Length < new_event.Constraints.Length)
                                {
                                    SortedEventCache.Insert(k, new_event);
                                    inserted = true;
                                    break;
                                }
                            }
                            if (!inserted)
                                SortedEventCache.Add(new_event);
                        }
                    }
                }
            }
        }

        public static void Emit(string EventName,params object[] args)
        {
            EventQueue.Enqueue((EventName, args));
        }

        public static void Poll()
        {
            while (EventQueue.Count > 0)
            {
                var (EventName, args) = EventQueue.Dequeue();
                HandleEvent(EventName, args);
            }
        }

        public static void SetConfig(ConfigFile config)
        {
            BetterEventHandler.config = config;
        }

        private static void HandleEvent(string event_name, params object[] args)
        {
            List<Event> event_shortlist = SortedEventCache.FindAll(e => e.EventId == event_name && e.method.GetParameters().Length == args.Length);

            event_shortlist.RemoveAll(e =>
            {
                bool match = true;
                ParameterInfo[] mArgs = e.method.GetParameters();
                for (int j = 0; j < mArgs.Length; j++)
                {
                    var c = args[j].GetType();
                    if (!mArgs[j].ParameterType.IsAssignableFrom(c))
                    {
                        match = false;
                        break;
                    }
                }
                return !match;
            });

            for (int i = 0; i < event_shortlist.Count; i++)
            {
                var mArgs = event_shortlist[i].method.GetParameters();
                Dictionary<string, object> variables = new Dictionary<string, object>();
                for (int j = 0; j < args.Length; j++)
                {
                    variables.Add(mArgs[j].Name, args[i]);
                }

                Stack<object> prog = new Stack<object>();
                for (int j = 0; j < event_shortlist[i].Constraints.Length; j++)
                {
                    for (int k = 0; k < event_shortlist[i].Constraints[j].Item2.Length; k++)
                    {
                        prog.Push(event_shortlist[i].Constraints[j].Item2[event_shortlist[i].Constraints[j].Item2.Length - 1 - k]);
                    }
                    prog.Push(event_shortlist[i].Constraints[j].Item1);
                }

                if (evaluate_stack(variables, prog))
                {
                    event_shortlist[i].method.Invoke(null, args);
                }
            }

        }

        private static bool evaluate_stack(Dictionary<string, object> variables, Stack<object> stack)
        {
            bool res = true;
            while (stack.Count > 0)
            {
                if (stack.Peek().GetType() == typeof(ConstraintType))
                {
                    try
                    {
                        ConstraintType type = (ConstraintType)stack.Pop();
                        if (type == ConstraintType.Eq)
                        {
                            if (stack.Peek().GetType() != typeof(string))
                                throw new Exception("Expected string!");
                            string a = (string)stack.Pop();

                            if (stack.Peek().GetType() != typeof(string))
                                throw new Exception("Expected string!");
                            string b = (string)stack.Pop();
                            
                            var A = get_value(variables, a);
                            var B = get_value(variables, b);
                            res = res && (A.Equals(B));
                        }
                        else if (type == ConstraintType.NEq)
                        {
                            if (stack.Peek().GetType() != typeof(string))
                                throw new Exception("Expected string!");
                            string a = (string)stack.Pop();

                            if (stack.Peek().GetType() != typeof(string))
                                throw new Exception("Expected string!");
                            string b = (string)stack.Pop();

                            res = res && (!get_value(variables, a).Equals(get_value(variables, b)));
                        }
                    }
                    catch
                    {
                        return false;
                    }

                    if (!res) return false;
                }
            }

            return res;
        }

        private static object get_value(Dictionary<string, object> variables, string val)
        {
            string[] fragments_raw = val.Split('.').Select(s => s.Trim()).ToArray();
            Queue<string> fragments = new Queue<string>();
            for (int i = 0; i < fragments_raw.Length; i++)
                fragments.Enqueue(fragments_raw[i]);

            object current_obj = null;
            while (fragments.Count > 0)
            {
                if (current_obj == null)
                    current_obj = resolve_value(variables, fragments.Dequeue());
                else
                {
                    string field_name = fragments.Dequeue();
                    FieldInfo field = current_obj.GetType().GetRuntimeField(field_name);
                    PropertyInfo property = current_obj.GetType().GetRuntimeProperty(field_name);
                    if (field == null && property == null)
                        throw new Exception($"Field {field_name} doesn't exist on object {current_obj.ToString()}");
                    if (field != null)
                        current_obj = field.GetValue(current_obj);
                    else if (property != null)
                        current_obj = property.GetValue(current_obj);
                }
            }

            return current_obj;
        }

        private static object resolve_value(Dictionary<string, object> variables, string val)
        {
            if (val == "$config")
                return config;
            else if (variables.ContainsKey(val))
                return variables[val];
            else throw new Exception($"Couldn't find {val}");
        }
    }

    public enum ConstraintType
    {
        Eq,
        NEq
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EventData : Attribute
    {
        public (ConstraintType, string)[] Constraints;
        public string EventId;

        public EventData(string EventId, params object[] Constraints)
        {
            this.EventId = EventId;
            if (Constraints.Length % 2 != 0)
                throw new Exception("Number of constraints must be even!");
            this.Constraints = new (ConstraintType, string)[Constraints.Length / 2];
            for (int i = 0; i < Constraints.Length; i++)
            {
                int index = (i / 2);

                if (i % 2 == 0)
                    this.Constraints[index].Item1 = (ConstraintType)Constraints[i];
                else
                    this.Constraints[index].Item2 = (string)Constraints[i];
            }
        }
    }
}
