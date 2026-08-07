using System.Collections.Generic;
using UnityEngine;

public class OnAllyAttackTrigger : Trigger
{
    public string playerId;

    public OnAllyAttackTrigger(string playerId, List<string> eventTypes): base(eventTypes)
    {
        this.playerId = playerId;
    }

    public override bool ShouldTrigger(Event eve, Game game)
    {
        AttackEvent attackEvent = (AttackEvent)eve;
        bool shoudlTrigger = attackEvent != null && attackEvent.attackingEntities.Find(entity => entity.controllerId == playerId) != null;
        return base.ShouldTrigger(eve, game) && shoudlTrigger;
    }
}
