using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Send cards from anywhere to the Grave. Has tag "Destroy".
/// </summary>
public class DestroyEvent : Event
{
    public List<Entity> entitiesToDestroy = new List<Entity>();
    public List<Entity> entitiesDestroyed = new List<Entity>();

    public DestroyEvent(List<Entity> entitiesToDestroy)
    {
        this.entitiesToDestroy = entitiesToDestroy;
        eventType = "destroy";
        eventTags.Add("destroy");
    }

    protected override async Task Execute(Game game)
    {
        await base.Execute(game);
        Dictionary<string, List<Entity>> playerEntities = new Dictionary<string, List<Entity>>();
        // Organize Entities by owner
        foreach (var entity in entitiesToDestroy)
        {
            string playerId = entity.controllerId;
            if (!playerEntities.ContainsKey(playerId))
                playerEntities.Add(playerId, new List<Entity>());
            playerEntities[playerId].Add(entity);
            entitiesDestroyed.Add(entity);
        }
        // Send each entity to their owners graveyard
        foreach (var playerId in playerEntities.Keys)
        {
            MoveToZoneEvent moveEvent = new MoveToZoneEvent(playerEntities[playerId], CommonZones.GRAVE, playerId);
            game.eventManager.AddEvent(moveEvent);
        }
    }
}
