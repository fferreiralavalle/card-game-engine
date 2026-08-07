using Mono.Cecil;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;


[MoonSharpUserData]
public class DealDamageEvent : ChangeResourceEvent
{
    public Damage damage;
    public List<Entity> targets = new List<Entity>();

    public DealDamageEvent(Damage damage, List<Entity> targets) : base(targets, new List<ResourceChange>() { new ResourceChange("health", new ResourceMod(-damage.amount)) })
    {
        this.damage = damage;
        this.targets = targets;
        eventTags.Add("damage");
    }

    protected override async Task Execute(Game game)
    {
        await base.Execute(game);
        string resourceAsHealthId = game.rules.resourceIdForHealth;
        List<Entity> kills = new List<Entity>();
        foreach (var entity in targets)
        {
            if (entity.resources.ContainsKey(resourceAsHealthId))
            {
                if (entity.resources[resourceAsHealthId].GetAmount() <= 0)
                {
                    kills.Add(entity);
                }
            }
        }
        output["kills"] = kills;
    }
}
