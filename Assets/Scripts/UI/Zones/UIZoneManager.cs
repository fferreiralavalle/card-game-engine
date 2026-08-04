using RuntimeCardEngine;
using System.Collections.Generic;
using UnityEngine;

public class UIZoneManager : MonoBehaviour
{
    public static UIZoneManager Instance;
    public CardGameplayEngine GameplayEngine;

    public Collider2D playZoneArea;

    public List<UIZone> zones = new List<UIZone> ();

    protected Game game;

    private void Awake()
    {
        Instance = this;
        GameplayEngine.onGamePrepared += (Game game) =>
        {
            this.game = game;
            foreach (var zone in game.zones)
            {
                UIZone uiZone = GetZone(zone.zoneCategory, zone.ownerId);
                uiZone?.Initialize(zone, game);
            }
        };
    }

    public UIZone GetZone(string zoneCategory, string ownerId)
    {
        return zones.Find(z => z.zoneCategory == zoneCategory && z.ownerId == ownerId);
    }

    public Game GetGame()
    {
        return GameplayEngine.game;
    }

    public bool IsInPlayZone(Vector3 position)
    {
        bool isInside = playZoneArea.bounds.Contains(position);

        return isInside;
    }

    public bool TryToPlay(UICardEntity card)
    {
        bool isInPlayZone = IsInPlayZone(card.transform.position);
        if (isInPlayZone)
        {
            GameplayEngine.game.AddEvent(new PlayEvent(card.Entity, new Dictionary<string, int>(), null));
            return true;
        }
        return false;
    }

    public void HandleAttack(Entity entity, Entity target)
    {
        AttackEvent attack = new AttackEvent(new List<Entity>() { entity }, target);
        game.AddEvent(attack);
    }
}
