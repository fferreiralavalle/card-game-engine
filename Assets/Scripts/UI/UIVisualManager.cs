using RuntimeCardEngine;
using System.Collections.Generic;
using UnityEngine;

public class UIVisualManager : MonoBehaviour
{
    public static UIVisualManager Instance;

    public UICardEntity cardEntityPrefab;
    public UITargetingArrow targetingArrow;

    public UIZoneManager zoneManager;

    public Dictionary<string, UICardEntity> spawnedEntities = new Dictionary<string, UICardEntity> ();

    public Game currentGame;

    private void Awake()
    {
        Instance = this;
        if (CardGameplayEngine.Instance)
            CardGameplayEngine.Instance.onGamePrepared += (g) => Initiate(g);
    }

    public UIVisualManager Initiate(Game game)
    {
        currentGame = game;
        return Instance;
    }

    public UICardEntity GetCardEntity(Entity entity, UICardEntity prefab = null)
    {
        if (!CardEntityExists(entity))
        {
            UICardEntity presetUsed = prefab ?? cardEntityPrefab;
            UICardEntity cardEntity = Instantiate(presetUsed).Initiate(entity);
            cardEntity.SubscribeToPropertyChanges(currentGame);
            spawnedEntities.Add(entity.runtimeId, cardEntity);
        }
        return spawnedEntities[entity.runtimeId];
    }

    public UICardEntity TransformCardEntityPreset(Entity entity, UICardEntity newPrefab)
    {
        if (CardEntityExists(entity) && newPrefab)
        {
            UICardEntity oldCardEntity = spawnedEntities[entity.runtimeId];
            UICardEntity newCardEntity = Instantiate(newPrefab).Initiate(entity);
            newCardEntity.transform.position = oldCardEntity.transform.position;
            spawnedEntities[entity.runtimeId] = newCardEntity;
            oldCardEntity.gameObject.SetActive(false);
            newCardEntity.SubscribeToPropertyChanges(currentGame);
            return newCardEntity;
        }
        else
        {
            return GetCardEntity(entity, newPrefab);
        }
    }

    public bool CardEntityExists(Entity entity)
    {
        return spawnedEntities.ContainsKey(entity.runtimeId);
    }

    public UIZone GetZone(string zoneCategory, string ownerId)
    {
        return zoneManager.GetZone(zoneCategory, ownerId);
    }

    public UITargetingArrow StartTargetingArrow(Transform originPoint)
    {
        targetingArrow.StartTargeting(originPoint);
        return targetingArrow;
    }

    public UITargetingArrow StopTargetingArrow()
    {
        targetingArrow.StopTargeting();
        return targetingArrow;
    }

    public UICardEntity GetHoveredEntity()
    {
        UICardEntity targetingEntity = UIEntityPicker.GetHoveredCardFromRaycast();
        return targetingEntity;
    }

    public void HandleEndTurn()
    {
        currentGame.AddEvent(new EndTurnEvent());
    }
}
