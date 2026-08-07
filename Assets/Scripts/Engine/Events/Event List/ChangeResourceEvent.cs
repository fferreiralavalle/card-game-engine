using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
[Serializable]
[MoonSharpUserData]
public class ChangeResourceEvent : Event
{
    public List<Entity> targetEntities = new List<Entity>();
    public List<ResourceChange> resourceChanges = new List<ResourceChange>();

    public ChangeResourceEvent(List<Entity> targetEntities, List<ResourceChange> resourceChanges)
    {
        this.targetEntities = targetEntities;
        this.resourceChanges = resourceChanges;
        eventType = "change_properties";
    }

    protected override async Task Execute(Game game)
    {
        await base.Execute(game);

        foreach (var entity in targetEntities)
        {
            foreach (var resource in resourceChanges)
            {
                if (entity.resources.ContainsKey(resource.resourceId))
                {
                    entity.resources[resource.resourceId].Modify(resource.resourceMod);
                }
            }
        }
    }
}
