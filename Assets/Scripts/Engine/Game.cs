
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// An instance of a game
/// </summary>
[Serializable]
public class Game
{
    public List<Player> players = new List<Player>();
    public List<Zone> zones = new List<Zone>();
    public EventManager eventManager = new EventManager();
    public GameRules rules = new GameRules();

    // Which player has priority right now
    public string activePlayerId = "";
    public int turns = 0;

    public Game (List<Player> players, List<Zone> zones, GameRules rules)
    {
        this.players = players;
        this.zones = zones;
        this.rules = rules;
        activePlayerId = players[0].playerId;
    }

    public List<Zone> GetAllZonesFromPlayer(string playerId) { return zones.FindAll(z => z.ownerId == playerId); }

    public Zone GetZoneFromPlayer(string zoneCategory, string playerId) { return zones.Find(z => z.zoneCategory == zoneCategory && z.ownerId == playerId); }

    public List<Zone> GetAllZonesWithCategory(string zoneCategory)
    {
        return zones.FindAll(z => z.zoneCategory == zoneCategory);
    }

    public List<Player> GetOpponents(string playerId)
    {
        return players.Where(p => p.playerId != playerId).ToList();
    }

    public List<Player> GetPlayers()
    {
        return players;
    }

    public Zone GetCardZone(string cardId)
    {
        foreach (Zone zone in zones)
        {
            Entity card = zone.entities.Find(e => e.runtimeId == cardId);
            if (card != null)
                return zone;
        }

        return null;
    }
    protected void HandleAllEvents()
    {
        eventManager.HandleAllEvents(this);
    }

    /// <summary>
    /// Adds event first (top of the stack)
    /// </summary>
    /// <param name="eve"></param>
    public void AddEvent(Event eve)
    {
        bool wasFree = !eventManager.IsBusy;
        eventManager.AddEvent(eve);
        if (wasFree)
            HandleAllEvents();
    }

    /// <summary>
    /// Adds event last (bottom of the stack)
    /// </summary>
    /// <param name="eve"></param>
    public void AddEventLast(Event eve)
    {
        bool wasFree = !eventManager.IsBusy;
        eventManager.AddEventLast(eve);
        if (wasFree)
            HandleAllEvents();
    }

    public void AddTrigger(Trigger trigger)
    {
        eventManager.AddTrigger(trigger);
    }

    public CreateEntityEvent CreateEntityEvent(string entityId, string zoneCateogry, string zoneOwnerId)
    {
        EntityData entityData = EntityTemplateManager.Instance.GetEntity(entityId);
        CreateEntityEvent moveEntityEvent = new (entityData, zoneCateogry, zoneOwnerId);
        AddEvent(moveEntityEvent);
        return moveEntityEvent;
    }

    public Entity CreateEntity(EntityData entityData, string zoneCateogry, string zoneOwnerId)
    {
        Entity createdEntity = new Entity(entityData);
        MoveToZoneEvent moveEntityEvent = new MoveToZoneEvent(new List<Entity> { createdEntity }, zoneCateogry, zoneOwnerId);
        AddEvent(moveEntityEvent);
        return createdEntity;
    }

    public int GetPlayerTurnsTaken(string playerId)
    {
        int index = players.FindIndex(p => p.playerId == playerId);
        if (index == -1) return 0;
        return (int) MathF.Floor(turns / (index + 1));
    }

    public int GetTurnsTaken()
    {
        return turns;
    }

    public string GetActivePlayer()
    {
        return activePlayerId;
    }

    public string GetNextPlayerTurn()
    {
        int index = players.FindIndex(p => p.playerId == GetActivePlayer());
        if (index == -1) return players.Count > 0 ? players[0].playerId : null;
        if (index == players.Count - 1) return players[0].playerId;
        return players[index+1].playerId;
    }

    public void SetActivePlayer(string playerId)
    {
        activePlayerId = playerId;
    }

    public int GetPlayerResourceAmount(string playerId, string resourceId)
    {
        Zone playerZone = GetZoneFromPlayer(rules.defaultPlayerResourceZone, playerId);
        if (playerZone == null) return 0;
        Entity playerEntity = playerZone.entities[0];
        Resource r = playerEntity.GetResource(resourceId);
        int amountAvailable = r != null ? r.GetAmount() : 0;
        return amountAvailable;
    }

    public int ModifyPlayerResource(string playerId, string resourceId, int amount)
    {
        Zone playerZone = GetZoneFromPlayer(rules.defaultPlayerResourceZone, playerId);
        if (playerZone == null) return 0;
        Entity playerEntity = playerZone.entities[0];
        Resource r = playerEntity.GetResource(resourceId);
        if (r == null) return 0;
        r.Modify(new ResourceMod(amount));
        return r.GetAmount();
    }

    public void ModifyPlayerResource(string playerId, string resourceId, ResourceMod modification)
    {
        Zone playerZone = GetZoneFromPlayer(rules.defaultPlayerResourceZone, playerId);
        if (playerZone == null) return;
        Entity playerEntity = playerZone.entities[0];
        Resource r = playerEntity.GetResource(resourceId);
        if (r == null) return;
        ChangeResourceEvent changeResource = new ChangeResourceEvent(new List<Entity>() { playerEntity }, new Dictionary<string, ResourceMod>() { {resourceId, modification } });
        AddEvent(changeResource);
    }

    public void PayCost(PlayCost cost, string playerId)
    {
        Zone playerZone = GetZoneFromPlayer(rules.defaultPlayerResourceZone, playerId);
        if (playerZone == null) return;
        Entity playerEntity = playerZone.entities[0];
        Dictionary<string, ResourceMod> resourcesChanged = new Dictionary<string, ResourceMod>();
        foreach (Resource resource in cost.costs.Values.ToList())
        {
            string resourceId = resource.resourceId;
            ResourceMod rm = new ResourceMod(-resource.GetAmount());
            resourcesChanged.Add(resourceId, rm);
        }
        if (resourcesChanged.Count > 0)
        {
            ChangeResourceEvent changeResource = new ChangeResourceEvent(new List<Entity>() { playerEntity }, resourcesChanged);
            AddEvent(changeResource);
        }
    }
}
