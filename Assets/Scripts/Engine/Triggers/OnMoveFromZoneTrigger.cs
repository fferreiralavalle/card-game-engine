using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Linq;

[MoonSharpUserData]
public class OnMoveFromZoneTrigger : Trigger
{
    public string originalZone;
    public string zoneOwnerId;
    public OnMoveFromZoneTrigger(string originalZone, string zoneOwnerId) : base(new List<string>() { EventUtils.Done("move_to_zone") })
    {
        this.originalZone = originalZone;
        this.zoneOwnerId = zoneOwnerId;
    }
    public override bool ShouldTrigger(Event eve, Game game)
    {
        MoveToZoneEvent eveZone = eve as MoveToZoneEvent;
        if (eveZone == null) return false;
        MoveZoneInfo matches = eveZone.moveZoneInfos.Values.ToList().Find(moveInfo => {
            Zone ogZone = moveInfo.originalZone;
            return ogZone != null && ogZone.zoneCategory == originalZone && ogZone.ownerId == zoneOwnerId;
        });
        return base.ShouldTrigger(eve, game) && matches != null;
    }
}
