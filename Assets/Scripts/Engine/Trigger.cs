using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;

[MoonSharpUserData]
[Serializable]
public class Trigger
{
    public int timesTriggered = 0;
    public int maxTriggers = 99999;

    /// <summary>
    /// List of event types this trigger affects. Use EventUtils to pick between completed or pending events
    /// </summary>
    public List<string> eventTypes = new List<string>();

    /// <summary>
    /// Source that is generating this trigger, not necessarily the target
    /// </summary>
    public Entity sourceEntity = null;

    public Action<Event, Trigger> onTrigger;

    public Trigger(List<string> eventTypes)
    {
        this.eventTypes = eventTypes;
    }

    public virtual bool ShouldTrigger(Event eve)
    {
        return timesTriggered < maxTriggers && !eve.isCancelled;
    }

    public bool CheckForTrigger(Event eve, Game currentGame)
    {
        if (ShouldTrigger(eve))
        {
            DoTrigger(eve, currentGame);
            return true;
        }

        return false;
    }

    public virtual void DoTrigger(Event eve, Game currentGame)
    {
        timesTriggered++;
        HandleTrigger(eve);
    }

    public void HandleTrigger(Event eve)
    {
        try
        {
            onTrigger?.Invoke(eve, this);
        }
        catch (ScriptRuntimeException ex)
        {
            UnityEngine.Debug.LogError($"[MoonSharp Callback Error]: {ex.DecoratedMessage}");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[C# Callback Error]: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void Subscribe(Action<Event, Trigger> handler)
    {
        onTrigger += handler;
    }

    public void Unsubscribe(Action<Event, Trigger> handler)
    {
        onTrigger -= handler;
    }
}
