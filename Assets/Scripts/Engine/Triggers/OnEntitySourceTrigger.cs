using MoonSharp.Interpreter;
using System.Collections.Generic;

[MoonSharpUserData]
public class OnEntitySourceTrigger : Trigger
{
    public List<string> matchingEntitiesRuntimeIds = new List<string> ();
    public OnEntitySourceTrigger(List<string> matchingEntitiesRuntimeIds, List<string> eventTypes) : base(eventTypes)
    {
        this.matchingEntitiesRuntimeIds = matchingEntitiesRuntimeIds;
    }

    public override void DoTrigger(Event eve, Game currentGame)
    {
        base.DoTrigger(eve, currentGame);
    }

    public override bool ShouldTrigger(Event eve)
    {
        // Only trigger for desired runtime entities
        return base.ShouldTrigger(eve) && eve.entitySource != null && matchingEntitiesRuntimeIds.Contains(eve.entitySource.runtimeId);
    }
}
