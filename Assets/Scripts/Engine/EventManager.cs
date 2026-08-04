using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class EventManager
{
    public List<Event> eventsQueue = new List<Event> ();

    /// <summary>
    /// A Dictionary with eventType as id and a list of Triggers as value
    /// </summary>
    public Dictionary<string, List<Trigger>> triggers = new Dictionary<string, List<Trigger>>();

    protected Event currentEvent = null;

    public bool IsBusy {
        get { return eventsQueue.Count > 0 || currentEvent != null; }
    }

    /// <summary>
    /// Adds event first in the line
    /// </summary>
    /// <param name="event"></param>
    public void AddEvent(Event @event) {
        int index = 0;
        foreach (Event ev in eventsQueue)
        {
            if (@event.priority < ev.priority)
            {
                continue;
            }
            index++;
        }
        eventsQueue.Insert(index, @event);
    }

    public virtual void AddEventLast(Event @event)
    {
        eventsQueue.Add(@event);
    }

    public virtual void AddTrigger(Trigger trigger)
    {
        foreach (string eventType in trigger.eventTypes)
        {
            if (triggers.ContainsKey(eventType))
            {
                triggers[eventType].Add(trigger);
            }
            else
            {
                triggers[eventType] = new List<Trigger>() { trigger };
            }
        }
    }

    /// <summary>
    /// Handles event execution and triggers for the first event in the List and removes it
    /// </summary>
    /// <param name="game"></param>
    public virtual async Task HandleNextEvent(Game game)
    {
        if (eventsQueue.Count == 0) return;
        currentEvent = eventsQueue[0];
        eventsQueue.RemoveAt(0);
        // How many events are in the stack after this one
        int eventsInQueue = eventsQueue.Count;
        // Check pending triggers
        string eventTypePending = EventUtils.Try(currentEvent.EventType);
        if (triggers.ContainsKey(eventTypePending))
        {
            foreach(Trigger trigger in triggers[eventTypePending])
            {
                trigger.CheckForTrigger(currentEvent, game);
            }
        }
        if (!currentEvent.isCancelled)
        {
            // Handle on try subscribers to this specific event
            currentEvent.HandleTry();
        }
        // If new events were added in response to this event, handle them first
        while (eventsQueue.Count > eventsInQueue)
        {
            await HandleNextEvent(game);
        }
        if (!currentEvent.isCancelled)
        {
            // Execute event
            await currentEvent.HandleExecute(game);
            // Check on Complete Triggers
            string eventTypeComplete = EventUtils.Done(currentEvent.EventType);
            if (triggers.ContainsKey(eventTypeComplete))
            {
                foreach (Trigger trigger in triggers[eventTypeComplete])
                {
                    trigger.CheckForTrigger(currentEvent, game);
                }
            }
            // Handle on done subscribers to this specific event
            currentEvent.HandleDone();
        }
        currentEvent = null;
    }

    public virtual async Task HandleAllEvents(Game game, int maxEvents = 9999)
    {
        int maxEventsCurrent = maxEvents;
        while (eventsQueue.Count > 0 && maxEventsCurrent > 0)
        {
            await HandleNextEvent(game);
            maxEventsCurrent--;
        }
        if (maxEventsCurrent == 0)
        {
            MonoBehaviour.print($"Max Events per Call reached! (${maxEvents}). There were still ${eventsQueue.Count} events left!");
            eventsQueue.Clear();
        }
    }
}
