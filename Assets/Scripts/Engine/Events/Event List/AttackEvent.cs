using System.Collections.Generic;
using System.Threading.Tasks;

public class AttackEvent : Event
{
    public Entity defendingEntity;
    public List<Entity> attackingEntities = new List<Entity>();
    public AttackEvent(List<Entity> attackingEntities, Entity defendingEntity)
    {
        this.defendingEntity = defendingEntity;
        this.attackingEntities = attackingEntities;

        eventType = "attack";
        eventTags.Add("combat");
    }

    protected override async Task Execute(Game game)
    {
        await base.Execute(game);
        foreach (var attacker in attackingEntities)
        {
            Resource attackR = attacker.GetResource(game.rules.resourceIdForDamage);
            Resource defenderR = defendingEntity.GetResource(game.rules.resourceIdForDamage);

            DealDamageEvent defenderDamageTaken = new DealDamageEvent(new Damage(attackR.GetAmount(), new List<string>() { "combat" }), new List<Entity>(){ defendingEntity });
            DealDamageEvent attackerDamageTaken = new DealDamageEvent(new Damage(defenderR.GetAmount(), new List<string>() { "combat" }), new List<Entity>() { attacker });

            defenderDamageTaken.eventTags = new List<string>() { "combat" };
            attackerDamageTaken.eventTags = new List<string>() { "combat" };

            game.eventManager.AddEvent(defenderDamageTaken);
            game.eventManager.AddEvent(attackerDamageTaken);
        }
    }
}
