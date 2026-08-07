using MoonSharp.Interpreter;
using System.Collections.Generic;

[MoonSharpUserData]
public class OnTurnStartEntityTrigger : OnTurnStartTrigger
{
    public List<string> zoneIds = new List<string>();
    public Entity entity;

    public OnTurnStartEntityTrigger(Entity entity, List<string> playerIds, List<string> zoneIds): base(playerIds)
    {
        this.entity = entity;
        this.playerIds = playerIds;
        this.zoneIds = zoneIds;
    }

    public override bool ShouldTrigger(Event eve, Game game)
    {
        Zone zone = game.GetCardZone(entity.runtimeId);
        if (zone != null && zoneIds.Contains(zone.zoneCategory))
        {
            return base.ShouldTrigger(eve, game);
        }
        return false;
    }
}
