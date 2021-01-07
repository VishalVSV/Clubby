using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Clubby.Events
{
    public class EventManager
    {
        private Mutex mutex = new Mutex();
        private Queue<Message> messages = new Queue<Message>();
        public delegate bool EventHandle(object data);//Return value signals whether to propagate the event or stop processing it
        private Dictionary<string, SortedList<int, EventHandle>> handles = new Dictionary<string, SortedList<int, EventHandle>>();

        public void Emit(string Name, object Data)
        {
            mutex.WaitOne();
            messages.Enqueue(new Message(Name, Data));
            mutex.ReleaseMutex();
        }
        public void On(string Event, EventHandle handle)
        {
            if (handles.ContainsKey(Event))
            {
                handles[Event].Add(handles[Event].Count, handle);
            }
            else
            {
                handles.Add(Event, new SortedList<int, EventHandle>() { { 0, handle } });
            }
        }

        public void On(string Event, EventHandle handle, int priority)
        {
            if (handles.ContainsKey(Event))
            { 
                handles[Event].Add(priority, handle);
            }
            else
            {
                handles.Add(Event, new SortedList<int, EventHandle>() { { priority, handle } });
            }
        }

        public void Poll()
        {
            mutex.WaitOne();

            while (messages.Count > 0)
            {

            }

            mutex.ReleaseMutex();
        }
    }

    public class Message
    {
        public string Name;
        public object Data;
        public Message(string name, object data)
        {
            Name = name;
            Data = data;
        }
    }
}
