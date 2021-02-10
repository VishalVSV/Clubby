using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.EventManagement
{
    /// <summary>
    /// The result enum returned by event handlers.
    /// </summary>
    public enum EventResult
    {
        Continue,
        Stop,
    }

    /// <summary>
    /// A generic event handler to bubble events to multiple consumers.
    /// </summary>
    /// <typeparam name="T">The data sent to the event</typeparam>
    public class EventManager<T>
    {
        /// <summary>
        /// The raw event data backing
        /// </summary>
        public Dictionary<string, List<(int, Func<T, EventResult>)>> events = new Dictionary<string, List<(int, Func<T, EventResult>)>>();

        /// <summary>
        /// Add a event listener.
        /// </summary>
        /// <param name="event_name">The name of the event to listen to.</param>
        /// <param name="handler">The actual listener</param>
        /// <param name="priority">The priority of the event</param>
        public void On(string event_name, Func<T, EventResult> handler, int priority = 5)
        {
            if (!events.ContainsKey(event_name))
                events.Add(event_name, new List<(int, Func<T, EventResult>)>());

            Insert(events[event_name], handler, priority);
        }

        /// <summary>
        /// Dispatch an event.
        /// </summary>
        /// <param name="event_name">The name of the event to dispatch</param>
        /// <param name="args">The arguments to send to the event listeners</param>
        public void Dispatch(string event_name,T args)
        {
            if(events.ContainsKey(event_name))
            {
                for (int i = 0; i < events[event_name].Count; i++)
                {
                    // If a event asks the handler to stop it won't continue to bubble the event to other listeners.
                    if(events[event_name][i].Item2(args) == EventResult.Stop)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// A sorted insert function.
        /// </summary>
        /// <param name="list">The list to insert into.</param>
        /// <param name="item">The item to insert</param>
        /// <param name="priority">The priority used to sort the tuples.</param>
        private void Insert(List<(int, Func<T, EventResult>)> list, Func<T, EventResult> item, int priority)
        {
            if (list.Count == 0)
            {
                list.Add((priority, item));
                return;
            }

            int i = 0;
            while (i < list.Count)
            {
                if (list[i].Item1 > priority)
                {
                    list.Insert(i, (priority, item));
                    return;
                }
                i++;
            }

            list.Add((priority, item));
        }
    }
}
