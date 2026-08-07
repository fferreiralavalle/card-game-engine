using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class UIPlayerResource : MonoBehaviour
{
    public string playerId;
    public string resourceId;

    public UIResource resource;

    public UIPlayerResource Initialize(Game game)
    {
        Resource resource = game.GetPlayerResources(playerId).Values.ToList().Find(r => r.resourceId == resourceId);
        if (resource != null)
        {
            this.resource.Initiate(resource);
        }
        SubscribeToPropertyChanges(game);
        return this;
    }

    public async Task UpdateAttributes()
    {
        await resource.UpdateResource();
    }

    public void SubscribeToPropertyChanges(Game game)
    {
        Entity playerEntity = game.GetPlayerEntity(playerId);
        OnEntityPropertyChangeTrigger onPropertyChange = new OnEntityPropertyChangeTrigger(playerEntity.runtimeId);
        onPropertyChange.onTrigger += (ev, trigger) =>
        {
            if (!ev.eventTags.Contains("combat"))
                UpdateAttributes();
        };
        game.AddTrigger(onPropertyChange);
    }
}
