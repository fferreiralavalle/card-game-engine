using DG.Tweening;
using RuntimeCardEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;

public class UIZoneField : UIZone
{
    [Header("On Combat Damage Screen Shake")]
    public float duration = 0.2f;
    public float minStrength = 10f;
    public float extraStrengthPerPointOfDamage = 2f;
    public UnityEvent<Entity, Entity> OnAttack;

    public override void HandleZoneMoveTriggers(Game game)
    {
        base.HandleZoneMoveTriggers(game);

        OnAllyAttackTrigger tryAllyAttackTrigger = new OnAllyAttackTrigger(ownerId, new List<string>() { EventUtils.Try("attack")});
        OnAllyAttackTrigger doneAllyAttackTrigger = new OnAllyAttackTrigger(ownerId, new List<string>() { EventUtils.Done("attack") });

        tryAllyAttackTrigger.onTrigger += HandleTryAttack;
        doneAllyAttackTrigger.onTrigger += HandlePerformAttack;

        game.AddTrigger(tryAllyAttackTrigger);
        game.AddTrigger(doneAllyAttackTrigger);

    }

    public void HandleTryAttack(Event @event, Trigger trigger)
    {
        AttackEvent ae = @event as AttackEvent;
        UICardEntity defender = UIVisualManager.Instance.GetCardEntity(ae.defendingEntity, zoneCardEntityPrefab);

        foreach (Entity attacker in ae.attackingEntities)
        {
            UICardEntity uiEntity = GetView(attacker);
            if (uiEntity != null)
            {
                UIEntitiyAttack uiEntityAttacker = uiEntity.GetOrAddComponent<UIEntitiyAttack>(); ;
                Func<Task> animationTask = async () =>
                {
                    await uiEntityAttacker.TryAttackTask(defender.transform);
                };
                VisualSequencer.Instance.EnqueueAnimation(animationTask);
            }
        }
    }

    /// <summary>
    /// Phase 2: Dash to target, trigger impact callback, return home
    /// </summary>
    public void HandlePerformAttack(Event @event, Trigger trigger)
    {
        AttackEvent ae = @event as AttackEvent;
        UICardEntity defender = UIVisualManager.Instance.GetCardEntity(ae.defendingEntity, zoneCardEntityPrefab);

        foreach (Entity attacker in ae.attackingEntities)
        {
            UICardEntity entity = GetView(attacker);
            if (entity != null)
            {
                UIEntitiyAttack uiEntityAttacker = entity.GetOrAddComponent<UIEntitiyAttack>();
                Func<Task> animationTask = async () =>
                {
                    await uiEntityAttacker.PerformAttackTask(defender.transform, () =>
                    {
                        entity.UpdateAttributes();
                        defender.UpdateAttributes();
                        UICameraShake.Instance.Shake(duration, minStrength);
                    });
                };
                VisualSequencer.Instance.EnqueueAnimation(animationTask);
            }
        }
    }

    public override async Task HandleAddCard(Entity entity, bool showPreview, bool overDraw)
    {
        UICardEntity entityUI = UIVisualManager.Instance.TransformCardEntityPreset(entity, zoneCardEntityPrefab);
        if (!overDraw)
        {
            entityUI.onDrag += HandleDrag;
            entityUI.onDrop += HandleDrop;
            entityUI.GetOrAddComponent<UIEntitiyAttack>();
        }
        await AddCardTask(entityUI.transform, showPreview, overDraw);
    }

    public override Task HandleCardsRemove(MoveToZoneEvent moveToZoneEvent)
    {
        foreach (MoveZoneInfo moveInfo in moveToZoneEvent.moveZoneInfos.Values.ToList())
        {
            Entity e = moveInfo.entity;
            UICardEntity uiEntity = GetView(e);
            if (uiEntity != null)
            {
                Destroy(uiEntity.GetComponent<UIEntitiyAttack>());
            }
        }
        return base.HandleCardsRemove(moveToZoneEvent);
    }

    public override void HandleDrag(UICardEntity uiEntity)
    {
        Transform cardPos = uiEntity.transform;
        if (!UIVisualManager.Instance.targetingArrow.isTargeting)
            UIVisualManager.Instance.StartTargetingArrow(cardPos);
    }

    public override void HandleDrop(UICardEntity uiEntity)
    {
        UIVisualManager.Instance.StopTargetingArrow();
        UICardEntity targetingEntity = UIEntityPicker.GetHoveredCardFromRaycast();
        if (targetingEntity != null && targetingEntity.Entity.controllerId != uiEntity.Entity.controllerId)
        {
            OnAttack?.Invoke(uiEntity.Entity, targetingEntity.Entity);
        }
    }
}
