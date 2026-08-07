using System.Collections.Generic;
using UnityEngine;

public class OnEntityRemoveResourceMod : Trigger
{
    public string entityRuntimeId = "";
    public OnEntityRemoveResourceMod(string entityRuntimeId) : base(new List<string>() { EventUtils.Done("remove_attribute_change") })
    {
        this.entityRuntimeId = entityRuntimeId;
    }

    public override bool ShouldTrigger(Event eve, Game game)
    {
        RemoveAttributeChangeEvent cre = (RemoveAttributeChangeEvent)eve;
        if (cre != null)
        {
            if (cre.targetEntities.Find(e => e.runtimeId == entityRuntimeId) != null)
            {
                return base.ShouldTrigger(eve, game);
            }
        }
        return false;
    }
}
