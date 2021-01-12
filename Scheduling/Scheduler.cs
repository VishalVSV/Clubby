using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Clubby.Scheduling
{
    /// <summary>
    /// Reminder event for the scheduler
    /// </summary>
    public class SchedulerEvent
    {
        /// <summary>
        /// The name of the event.
        /// </summary>
        public string name;

        /// <summary>
        /// The channel to send the reminder on
        /// </summary>
        public ulong channel;
        /// <summary>
        /// The message to send
        /// </summary>
        public string message;
        /// <summary>
        /// The embed to use to send the message
        /// </summary>
        public EmbedBuilder embed;
    }

    /// <summary>
    /// Event that is called periodically by the scheduler
    /// </summary>
    public class PeriodicEvent
    {
        /// <summary>
        /// Time that the event last executed.
        /// </summary>
        public DateTime last_exec;
        /// <summary>
        /// The interval in which the event is called
        /// </summary>
        public TimeSpan interval;
        /// <summary>
        /// The action to call every interval
        /// </summary>
        public Action action;
    }

    /// <summary>
    /// Schedules and executes events
    /// </summary>
    public class Scheduler
    {
        /// <summary>
        /// The raw data for the scheduler
        /// </summary>
        /// It is a dictionary where the key is the category of events and the values are a sorted list of tuples containing the event and its id.
        public Dictionary<string, SortedList<DateTime, (int, SchedulerEvent)>> Scheduler_raw = new Dictionary<string, SortedList<DateTime, (int, SchedulerEvent)>>();

        /// <summary>
        /// The events to execute periodically
        /// </summary>
        /// This field must never be serialized nor persistently retained. It may point to closures in unloaded assemblies.
        [JsonIgnore]
        public List<PeriodicEvent> PeriodicActivities = new List<PeriodicEvent>();

        /// <summary>
        /// Creates a category to schedule events in
        /// </summary>
        /// <param name="category">The name of the category</param>
        public void CreateCategory(string category)
        {
            if (!Scheduler_raw.ContainsKey(category))
                Scheduler_raw.Add(category, new SortedList<DateTime, (int, SchedulerEvent)>());
        }

        /// <summary>
        /// Schedule a reminder event
        /// </summary>
        /// <param name="category">The name of the category to assign the event to</param>
        /// <param name="id">The id of the event</param>
        /// <param name="event">The reminder event object</param>
        /// <param name="time_to_exec">The time to send the reminder</param>
        public void Schedule(string category, int id, SchedulerEvent @event, DateTime time_to_exec)
        {
            // Ensure that the category exists before assigning
            CreateCategory(category);

            // Assign the event properly (Time to execute, (id of the event, the event))
            Scheduler_raw[category].Add(time_to_exec, (id, @event));
        }

        /// <summary>
        /// Reschedule an event.
        /// </summary>
        /// <param name="category">The name of the category</param>
        /// <param name="id">The id of the event to reschedule</param>
        /// <param name="time_to_exec">The new time to send the reminder</param>
        public void ReSchedule(string category, int id, DateTime time_to_exec)
        {
            // Ensure that the category exists (This is just a lazy workaround so I won't have to write one if statement, yes I'm lazy)
            CreateCategory(category);

            // Loop through and check if there is an event that matches the id
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

        /// <summary>
        /// Cancel a reminder event.
        /// </summary>
        /// <param name="category">The name of the category</param>
        /// <param name="id">The id of the reminder event</param>
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

        /// <summary>
        /// Tick the scheduler once
        /// </summary>
        /// This function checks all the events and executes the ones that need execution.
        /// It does not need to be ticked consistently.
        public void Tick()
        {
            // Get current time once to prevent having to call this again and again
            DateTime now = DateTime.Now;

            // Loop through all the events and check if they should be executed
            foreach (var events in Scheduler_raw.Values)
            {
                while (events.Count > 0)
                {
                    // If the time to execute has passed send the reminder and remove it from the list of events pending
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

            // Loop through all periodic activities and check if they should be executed
            for (int i = 0; i < PeriodicActivities.Count; i++)
            {
                if((now - PeriodicActivities[i].last_exec) > PeriodicActivities[i].interval)
                {
                    PeriodicActivities[i].action?.Invoke();
                    // Important: If this isn't reset it will repeatedly execute.
                    PeriodicActivities[i].last_exec = now;
                }
            }
        }
    }
}
