using MoonSharp.Interpreter;
using System.Collections.Generic;

[MoonSharpUserData]
public class OnMoveToZoneTrigger : Trigger
{
    public string targetZone;
    public string zoneOwnerId;
    public OnMoveToZoneTrigger(string targetZone, string zoneOwnerId) : base(new List<string>() { EventUtils.Done("move_to_zone") })
    {
        this.targetZone = targetZone;
        this.zoneOwnerId = zoneOwnerId;
    }
    public override bool ShouldTrigger(Event eve, Game game)
    {
        MoveToZoneEvent eveZone = eve as MoveToZoneEvent;
        if (eveZone == null) return false;
        bool matches = eveZone.zoneCategory == targetZone && eveZone.zoneOwnerId == zoneOwnerId;
        return base.ShouldTrigger(eve, game) && matches;
    }
}
