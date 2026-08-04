using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Serializable]
[MoonSharpUserData]
public class Event
{
    public int executeAmount = 1;
    public bool isCancelled = false;
    /// <summary>
    /// Use for accesing variables in LUA nodes
    /// </summary>
    public Dictionary<string, object> output = new Dictionary<string, object>();

    public string EventType { get { return eventType; } private set { eventType = value; } }

    /// <summary>
    /// Use it to identify things such as Battlecries, Deathrattle, draw, discard, etc
    /// </summary>
    public List<string> eventTags = new List<string>();

    /// <summary>
    /// Source that is generating this event, not necessarily the target
    /// </summary>
    public Entity entitySource = null;

    /// <summary>
    /// Unique per Event Class
    /// </summary>
    protected string eventType = "";

    /// <summary>
    /// Used for ordering events on stack, higher priority goes first
    /// </summary>
    public float priority = 0;

    public event Action<Event> OnTry;
    public event Action<Event> OnDone;

    public Event() {}

    /// <summary>
    /// The Event actions go here
    /// </summary>
    protected virtual async Task Execute(Game game)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Use Execute to handle the action. If you are a dev modify Execute instead.
    /// </summary>
    public virtual async Task HandleExecute(Game game)
    {
        if (isCancelled) return;
        for (int i = 0; i < executeAmount; i++)
        {
            // Check cancellation inside the loop in case a prior iteration
            // triggered a reaction that cancelled subsequent ones (e.g. Divine Shield pop)
            if (isCancelled) break;

            await Execute(game);
        }
    }

    public void HandleTry()
    {
        OnTry?.Invoke(this);
    }

    public void HandleDone()
    {
        try
        {
            OnDone?.Invoke(this);
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

    public virtual void SetOutput()
    {

    }

    public void SubscribeToDone(Action<Event> handler)
    {
        OnDone += handler;
    }
}
