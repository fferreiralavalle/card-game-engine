using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;

[MoonSharpUserData]
[Serializable]
public class OnEntityPropertyChangeTrigger : Trigger
{
    public string entityRuntimeId = "";
    public OnEntityPropertyChangeTrigger(string entityRuntimeId): base(new List<string>() { EventUtils.Done("change_properties") })
    {
        this.entityRuntimeId = entityRuntimeId;
    }

    public override bool ShouldTrigger(Event eve, Game game)
    {
        ChangeResourceEvent cre = (ChangeResourceEvent)eve;
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
