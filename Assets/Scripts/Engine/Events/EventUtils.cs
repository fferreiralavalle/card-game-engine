using UnityEngine;

public static class EventUtils
{
    public static string complete = "done";
    public static string pending = "try";

    /// <summary>
    /// A complete event has an EventResult assigned
    /// </summary>
    /// <param name="eventType"></param>
    /// <returns>the eventType + ".complete"</returns>
    public static string Done(string eventType)
    {
        return $"{eventType}.{complete}";
    }
    /// <summary>
    /// A complete event has an EventResult assigned
    /// </summary>
    /// <param name="eventType"></param>
    /// <returns>the eventType + ".pending"</returns>
    public static string Try(string eventType)
    {
        return $"{eventType}.{pending}";
    }

    public static string GetOriginalEventType(string eventType)
    {
        if (eventType.EndsWith($".{complete}"))
        {
            return eventType.Remove(eventType.Length - $".{complete}".Length);
        }
        else if (eventType.EndsWith($".{pending}"))
        {
            return eventType.Remove(eventType.Length - $".{pending}".Length);
        }
        return eventType;
    }
}
