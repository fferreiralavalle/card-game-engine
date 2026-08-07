using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;

[MoonSharpUserData]
public class TargetEvent : Event
{
    public List<Entity> validTargets = new List<Entity> ();

    public List<Entity> chosenEntities = new List<Entity>();

    TaskCompletionSource<UICardEntity> eventCompletionSource = new TaskCompletionSource<UICardEntity>();

    public TargetEvent(List<Entity> validTargets, Entity sourceEntity): base()
    {
        this.validTargets = validTargets;
        this.entitySource = sourceEntity;
        eventType = "target";
        SetOutput();
    }

    protected override async Task Execute(Game game)
    {
        if (validTargets.Count == 0)
            return;
        
        // If only one possible target skip selection and pick it
        if (validTargets.Count == 1)
        {
            chosenEntities = validTargets;
        }
        else
        {
            UICardEntity cardEntity = UIVisualManager.Instance.GetCardEntity(entitySource);

            UITargetingArrow targetingArrow =  UIVisualManager.Instance.StartTargetingArrow(cardEntity.transform);

            targetingArrow.onTargetChoosen += HandleChooseTarget;

            await eventCompletionSource.Task;
            targetingArrow.StopTargeting();
            targetingArrow.onTargetChoosen -= HandleChooseTarget;
        }
        SetOutput();
    }

    protected void HandleChooseTarget(UICardEntity target, UITargetingArrow _)
    {
        if (target != null)
        {
            Entity targetEntity = validTargets.Find(vt => vt.runtimeId == target.Entity.runtimeId);
            if (targetEntity != null)
            {
                chosenEntities = new List<Entity>() { targetEntity };
                eventCompletionSource.SetResult(target);
            }
        }
    }

    public override void SetOutput()
    {
        base.SetOutput();
        output["targets"] = chosenEntities;
    }
}
