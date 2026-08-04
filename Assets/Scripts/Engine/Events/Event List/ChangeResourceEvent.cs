using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
[Serializable]
public class ChangeResourceEvent : Event
{
    public List<Entity> targetEntities = new List<Entity>();
    public Dictionary<string, ResourceMod> resourceMods = new Dictionary<string, ResourceMod>();

    public ChangeResourceEvent(List<Entity> targetEntities, Dictionary<string, ResourceMod> resourceMod)
    {
        this.targetEntities = targetEntities;
        resourceMods = resourceMod;
        eventType = "change_properties";
    }

    protected override async Task Execute(Game game)
    {
        await base.Execute(game);

        foreach (var entity in targetEntities)
        {
            foreach (var resource in resourceMods)
            {
                if (entity.resources.ContainsKey(resource.Key))
                {
                    entity.resources[resource.Key].Modify(resource.Value);
                }
            }
        }
    }
}
