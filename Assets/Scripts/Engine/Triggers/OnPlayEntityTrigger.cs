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
    public override bool ShouldTrigger(Event eve, Game game)
    {
        bool matchesId = ((PlayEvent)eve).playedEntity.runtimeId == runtimeId;
        return base.ShouldTrigger(eve, game) && eve is PlayEvent && matchesId;
    }
}
