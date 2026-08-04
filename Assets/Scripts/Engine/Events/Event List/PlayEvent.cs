using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// When something is played it's cost is payed, then it goes to the stack
/// </summary>
[MoonSharpUserData]
public class PlayEvent: Event
{
    public Entity playedEntity;
    public Dictionary<string, int> resourcesPayed = new Dictionary<string, int>();
    public PlayCost costUsed;
    public Zone originZone;

    public PlayEvent(Entity playedEntity, Dictionary<string, int> resourcesPayed, PlayCost costUsed)
    {
        this.playedEntity = playedEntity;
        this.resourcesPayed = resourcesPayed;
        this.costUsed = costUsed;
        eventType = "play_entity";
        eventTags.Add("play");
    }

    protected override async Task Execute(Game game)
    {
        originZone = game.GetCardZone(playedEntity.runtimeId);
        await base.Execute(game);
        /*MoveToZoneEvent moveToStack = new MoveToZoneEvent(new List<Entity>() { playedEntity }, game.rules.defaultPlayZone, "");
        game.eventManager.AddEvent(moveToStack);*/
        // Move to desired zone
        EntityTypeData entityTypeData = EntityTypeManager.Instance.GetEntityType(playedEntity.GetCardTypes()[0]);
        string afterPlayZone = entityTypeData?.afterPlayZone ?? game.rules.defaultPlayZone;
        MoveToZoneEvent moveToPlay = new MoveToZoneEvent(new List<Entity>() { playedEntity }, afterPlayZone, playedEntity.controllerId);
        game.AddEvent(moveToPlay);
    }

    public override void SetOutput()
    {
        base.SetOutput();
        output["entity"] = playedEntity;
        output["resourcesPayed"] = resourcesPayed;
        output["originZone"] = originZone;
        output["playCost"] = costUsed;

    }
}
