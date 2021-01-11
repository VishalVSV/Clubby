using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.Scheduling
{
    public class SchedulerEvent
    {
        public string name;

        public ulong channel;
        public string message;
        public EmbedBuilder embed;
    }

    public class PeriodicEvent
    {
        public DateTime last_exec;
        public TimeSpan interval;
        public Action action;
    }

    public class Scheduler
    {
        public Dictionary<string, SortedList<DateTime, (int, SchedulerEvent)>> Scheduler_raw = new Dictionary<string, SortedList<DateTime, (int, SchedulerEvent)>>();

        [JsonIgnore]
        public List<PeriodicEvent> PeriodicActivities = new List<PeriodicEvent>();

        public void CreateCategory(string category)
        {
            if (!Scheduler_raw.ContainsKey(category))
                Scheduler_raw.Add(category, new SortedList<DateTime, (int, SchedulerEvent)>());
        }

        public void Schedule(string category, int id, SchedulerEvent @event, DateTime time_to_exec)
        {
            CreateCategory(category);

            Scheduler_raw[category].Add(time_to_exec, (id, @event));
        }

        public void ReSchedule(string category, int id, DateTime time_to_exec)
        {
            CreateCategory(category);

            var list = Scheduler_raw[category];
            for (int i = 0; i < list.Count; i++)
            {
                if(list.Values[i].Item1 == id)
                {
                    var key = list.Keys[i];
                    var e = list[key];
                    list.Remove(key);
                    list.Add(time_to_exec, e);
                }
            }
        }

        public void CancelEvent(string category, int id)
        {
            for (int i = 0; i < Scheduler_raw[category].Count; i++)
            {
                if (Scheduler_raw[category].Values[i].Item1 == id)
                {
                    Scheduler_raw[category].Remove(Scheduler_raw[category].Keys[i]);
                }
            }
        }

        public void Tick()
        {
            DateTime now = DateTime.Now;

            foreach (var events in Scheduler_raw.Values)
            {
                while (events.Count > 0)
                {
                    if (events.Keys[0] < now)
                    {
                        var e = events.Values[0].Item2;
                        try
                        {
                            SocketTextChannel channel = Program.config.GetChannel(e.channel);
                            channel.SendMessageAsync(e.message, false, e.embed.Build());
                        }
                        catch (Exception) { }
                        events.RemoveAt(0);
                    }
                    else break;
                }
            }


            for (int i = 0; i < PeriodicActivities.Count; i++)
            {
                if((now - PeriodicActivities[i].last_exec) > PeriodicActivities[i].interval)
                {
                    PeriodicActivities[i].action?.Invoke();
                    PeriodicActivities[i].last_exec = now;
                }
            }
        }
    }
}
