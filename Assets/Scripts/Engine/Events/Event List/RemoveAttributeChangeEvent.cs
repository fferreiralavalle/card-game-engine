using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
[MoonSharpUserData]
public class RemoveAttributeChangeEvent : Event
{
    public List<Entity> targetEntities = new List<Entity>();
    public List<string> attributeChangeIds = new List<string>();

    public RemoveAttributeChangeEvent(List<Entity> targetEntities, List<string> attributeChangeIds)
    {
        this.targetEntities = targetEntities;
        this.attributeChangeIds = attributeChangeIds;
        eventType = "remove_attribute_change";
    }

    protected override Task Execute(Game game)
    {
        foreach(Entity entity in targetEntities)
        {
            foreach(string attributeChangeId in attributeChangeIds)
            {
               entity.resources.Values.ToList().ForEach(mod => mod.RemoveModification(attributeChangeId));
            }
        }
        return base.Execute(game);
    }
}
