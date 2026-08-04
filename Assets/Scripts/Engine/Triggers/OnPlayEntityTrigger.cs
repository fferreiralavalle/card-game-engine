using MoonSharp.Interpreter;
using System.Collections.Generic;

[MoonSharpUserData]
public class OnPlayEntityTrigger : Trigger
{
    public string runtimeId;
    public OnPlayEntityTrigger(string runtimeId, List<string> eventCategories): base(eventCategories)
    {
        this.runtimeId = runtimeId;
    }
    public override bool ShouldTrigger(Event eve)
    {
        bool matchesId = ((PlayEvent)eve).playedEntity.runtimeId == runtimeId;
        return base.ShouldTrigger(eve) && eve is PlayEvent && matchesId;
    }
}
