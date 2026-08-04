using NUnit.Framework;
using RuntimeCardEngine;
using System.Collections.Generic;
using System.Linq;
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
        PlayabilityResult result = PlayabilityManager.Instance.CheckCanPlay(game, card.Entity);
        if (isInPlayZone && result.IsPlayable)
        {
            GameplayEngine.game.AddEvent(new TryToPlayEvent(card.Entity, card.Entity.GetPlayCosts()[0]));
            return true;
        }
        print(string.Join(", ", result.Reasons));
        return false;
    }

    public void HandleAttack(Entity entity, Entity target)
    {
        AttackEvent attack = new AttackEvent(new List<Entity>() { entity }, target);
        game.AddEvent(attack);
    }
}
