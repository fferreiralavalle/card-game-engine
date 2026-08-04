using RuntimeCardEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// This event checks if an entity can be played.
/// If it can, it pays the cost and queues the PlayEvent.
/// </summary>
public class TryToPlayEvent : Event
{
    public Entity entity;
    public PlayCost playCost;
    public PlayabilityResult playabilityResult;

    public TryToPlayEvent(Entity entity, PlayCost playCost)
    {
        this.entity = entity;
        this.playCost = playCost;
        eventType = "try_to_play_entity";
    }

    protected override Task Execute(Game game)
    {
        playabilityResult =  PlayabilityManager.Instance.CheckCanPlay(game, entity);
        if (playabilityResult.IsPlayable)
        {
            game.PayCost(playCost, entity.controllerId);
            Dictionary<string, int> resources = new Dictionary<string, int>();
            game.AddEvent(new PlayEvent(entity, playCost.GetTotalCost(), playCost));
        }
        return base.Execute(game);
    }
}
