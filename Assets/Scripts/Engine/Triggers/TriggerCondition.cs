using RuntimeCardEngine;
using System;
using UnityEngine;

public class TriggerCondition
{
    private readonly Func<Event, bool> _func;
    public int Priority { get; }

    public TriggerCondition(Func<Event, bool> func, int priority = 0)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        Priority = priority;
    }

    public bool Check(Event eve) => _func(eve);
}
