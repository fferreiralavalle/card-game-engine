using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Serializable]
[MoonSharpUserData]
public class MoveToZoneEvent : Event
{
    public List<Entity> targetEntities = new List<Entity> ();
    public string zoneCategory = "";
    public string zoneOwnerId = null;
    public List<Entity> movedEntities = new List<Entity>();
    public List<Entity> overflownEntities = new List<Entity> ();
    public Dictionary<string, MoveZoneInfo> moveZoneInfos = new Dictionary<string, MoveZoneInfo>();

    public MoveToZoneEvent(List<Entity> targetEntities, string zoneCategory, string zoneOwnerId)
    {
        this.targetEntities = targetEntities;
        this.zoneCategory = zoneCategory;
        this.zoneOwnerId = zoneOwnerId;
        eventType = "move_to_zone";
    }

    public MoveToZoneEvent(List<Entity> targetEntities, List<string> zoneCategory, List<string> zoneOwnerId)
    {
        this.targetEntities = targetEntities;
        this.zoneCategory = zoneCategory[0];
        this.zoneOwnerId = zoneOwnerId[0];
        eventType = "moveToZone";
    }

    protected override async Task Execute(Game game)
    {
        await base.Execute(game);
        // Remove entities from old zones
        Zone targetZone = game.GetZoneFromPlayer(zoneCategory, zoneOwnerId);
        foreach(Entity e in  targetEntities)
        {
            Zone currentZone = game.GetCardZone(e.runtimeId);
            if (currentZone != null) currentZone.entities.Remove(e);
            moveZoneInfos.Add(e.runtimeId, new MoveZoneInfo(e, currentZone, targetZone));
        }

        int cardsInZone = targetZone.entities.Count();
        int maxCardsInZone = targetZone.maxEntityAmountPerPlayer;
        int cardsToAddToZone = targetEntities.Count;
        int cardDiff = cardsInZone + cardsToAddToZone - maxCardsInZone;
        // Check if cards fit zone
        if (cardDiff <= 0) // Negative or 0 means all good
        {
            targetZone.entities.AddRange(targetEntities);
            movedEntities.AddRange(targetEntities);
        }
        else
        {
            int addableCards = maxCardsInZone - cardsInZone;
            List<Entity> cardsDrawn = targetEntities.Take(addableCards).ToList();
            overflownEntities = targetEntities.Skip(addableCards).ToList();
            movedEntities.AddRange(cardsDrawn);
            targetZone.entities.AddRange(cardsDrawn);
            foreach (Entity e in overflownEntities)
            {
                string defaultOverflowZoneId = game.rules.defaultOverflowZone;
                MoveToZoneEvent overDrawnMove = new MoveToZoneEvent(new List<Entity>() { e }, defaultOverflowZoneId, e.controllerId);
                game.AddEvent(overDrawnMove);
            }
        }
        if (!string.IsNullOrEmpty(targetZone.ownerId))
        {
            foreach(Entity entity in targetEntities)
            {
                // TODO check if zone owner != old controller and create an event to change ownership
                entity.controllerId = targetZone.ownerId;
            }
        }
    }

    public MoveZoneInfo GetEntityZoneInfo(string entityRunetimeId)
    {
        return moveZoneInfos[entityRunetimeId];
    }
}
