using MoonSharp.Interpreter;
using Newtonsoft.Json;
using RuntimeCardEngine;
using RuntimeNodeEditor;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
[MoonSharpUserData]
public class Entity
{
    /// <summary>
    /// Unique id for runtime entity
    /// </summary>
    public string runtimeId;
    public string entityName;
    public RuntimeCardEffectsData runtimeCardEffects;
    /// <summary>
    /// This can represent Attack, Health, Mana for players, Victory Points, etc
    /// </summary>
    public Dictionary<string, Resource> resources = new Dictionary<string, Resource>();
    /// <summary>
    /// Entities can have multiple ways of playing for them
    /// </summary>
    public List<PlayCost> costs = new List<PlayCost>();
    public List<string> cardTypeIds = new List<string>();
    /// <summary>
    /// Who has control of the Entity
    /// </summary>
    public string controllerId = null;
    /// <summary>
    /// Used for tracking effects and summoning sickness
    /// </summary>
    public int turnsInZone = 0;


    public Resource GetResource(string resourceId)
    {
        Resource res = resources[resourceId];
        return res;
    }

    public List<Resource> GetProperties()
    {
        return resources.Values.ToList().OrderBy(r =>
        {
            ResourceData rd = ResourceManager.Instance.GetResource(r.resourceId);
            return rd.orderPriority;
        }).ToList();
    }

    public List<string> GetCardTypes()
    {
        return cardTypeIds;
    }

    public List<PlayCost> GetPlayCosts()
    {
        return costs;
    }

    public Entity() { }

    public Entity(EntityData entityData)
    {
        runtimeId = Guid.NewGuid().ToString();
        entityName = entityData.name;
        runtimeCardEffects = new RuntimeCardEffectsData(entityData);
        foreach (var r in entityData.resources.Values)
        {
            resources.Add(r.resourceId, new Resource(r.resourceId, r.initialAmount));
        }
        costs = new List<PlayCost>(entityData.costs);
        cardTypeIds = new(entityData.cardTypeIds);
    }

    public int GetPropertyValue(string resourceId)
    {
        if (resources.ContainsKey(resourceId))
        {
            return resources[resourceId].GetAmount();
        }
        return 0;
    }
}
