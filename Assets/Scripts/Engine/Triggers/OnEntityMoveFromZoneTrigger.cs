using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Linq;

[MoonSharpUserData]
public class OnEntityMoveFromZoneTrigger : Trigger
{
    public List<Entity> entities;
    public List<string> originalZones;
    public OnEntityMoveFromZoneTrigger(List<Entity> entities, List<string> originalZones) : base(new List<string>() { EventUtils.Done("move_to_zone") })
    {
        this.entities = entities;
        this.originalZones = originalZones;
    }
    public override bool ShouldTrigger(Event eve, Game game)
    {
        MoveToZoneEvent eveZone = eve as MoveToZoneEvent;
        if (eveZone == null) return false;
        foreach(Entity entity in entities)
        {
            MoveZoneInfo matches = eveZone.moveZoneInfos.Values.ToList().Find(moveInfo => {
                Zone ogZone = moveInfo.originalZone;
                return ogZone != null && originalZones.Contains(ogZone.zoneCategory) && entity.runtimeId == moveInfo.entity.runtimeId;
            });
            return base.ShouldTrigger(eve, game) && matches != null;
        }
        return false;
    }
}
