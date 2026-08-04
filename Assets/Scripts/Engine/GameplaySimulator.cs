using RuntimeCardEngine;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class GameplaySimulator : MonoBehaviour
{
    public CardGameplayEngine gameplayEngine;
    public Game game;


    public List<string> player1EntitiesInDeck = new List<string>();
    public List<string> player2EntitiesInDeck = new List<string>();

    public int startingHand = 4;

    void Start()
    {
        Zone deck1 = game.zones.Find(z => z.zoneCategory == CommonZones.DECK && z.ownerId == "1");
        Zone deck2 = game.zones.Find(z => z.zoneCategory == CommonZones.DECK && z.ownerId == "2");

        foreach(string entityId in player1EntitiesInDeck)
        {
            EntityData entityData = EntityTemplateManager.Instance.GetEntity(entityId);
            Entity entity = new Entity(entityData);
            entity.controllerId = "1";
            deck1.entities.Add(entity);
        }
        foreach (string entityId in player2EntitiesInDeck)
        {
            EntityData entityData = EntityTemplateManager.Instance.GetEntity(entityId);
            Entity entity = new Entity(entityData);
            entity.controllerId = "2";
            deck2.entities.Add(entity);
        }

        gameplayEngine.InitializeGame(game);

        DrawEvent draw1 = new DrawEvent(startingHand, "1", "1");
        DrawEvent draw2 = new DrawEvent(startingHand, "2", "2");

        gameplayEngine.game.AddEvent(draw1);
        gameplayEngine.game.AddEvent(draw2);

        Zone p2Deck = game.GetZoneFromPlayer(CommonZones.DECK, "2");
        Entity ent2 = p2Deck.entities.Find(e => e.runtimeCardEffects.EntityData.entityId == "Reaper");

        PlayEvent playEvent2 = new PlayEvent(ent2, new Dictionary<string, int>(), new PlayCost());

        gameplayEngine.game.AddEvent(playEvent2);
    }


}
