using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICardEntity : MonoBehaviour
{
    public Entity Entity { get; private set; }

    public bool isHidden = false;

    public TextMeshProUGUI entityName;
    public Image art;
    public TextMeshProUGUI effect;
    public UIPlayCost playCostPrefab;
    public UIResource resourcePrefab;
    public RectTransform playCostContainer;
    public RectTransform attributesContainer;

    public Action<UICardEntity> onDrag;
    public Action<UICardEntity> onDrop;

    public UICardEntity Initiate(Entity entity)
    {
        Entity = entity;
        entityName.text = entity.entityName;
        effect.text = entity.runtimeCardEffects.EntityData.effect;

        UpdateCosts();
        UpdateAttributes();

        LoadImage(entity.runtimeCardEffects.EntityData.artPath);

        return this;
    }

    public async Task LoadImage(string path)
    {
        Sprite image = await AssetsManager.Instance.GetSpriteAsync(path);
        if (image && art)
        {
            art.sprite = image;
        }
    }

    public void SubscribeToPropertyChanges(Game game)
    {
        OnEntityPropertyChangeTrigger onPropertyChange = new OnEntityPropertyChangeTrigger(Entity.runtimeId);
        onPropertyChange.onTrigger += (ev, trigger) =>
        {
            if (!ev.eventTags.Contains("combat"))
                UpdateAttributes();
        };
        game.AddTrigger(onPropertyChange);
    }

    public void UpdateCosts()
    {
        foreach(Transform t in playCostContainer.transform)
        {
            Destroy(t.gameObject);
        }

        foreach(PlayCost pc in Entity.GetPlayCosts())
        {
            Instantiate(playCostPrefab, playCostContainer).Initiate(pc);
        }
    }

    public void UpdateAttributes()
    {

        foreach (Transform t in attributesContainer.transform)
        {
            Destroy(t.gameObject);
        }

        EntityTypeData etd = EntityTypeManager.Instance.GetEntityType(Entity.GetCardTypes().First());
        foreach (Resource r in Entity.GetProperties())
        {
            EntityTypeResource rd = etd.entityTypeResources.Find(etr => etr.resourceId == r.resourceId);
            if (!rd.hideInEntityUi)
                Instantiate(resourcePrefab, attributesContainer).Initiate(r);
        }
    }

    public void SetVisibility(bool isHidden)
    {
        this.isHidden = isHidden;
    }

    public void HandleDrag()
    {
        onDrag?.Invoke(this);
    }

    public void HandleDrop()
    {
        onDrop?.Invoke(this);
    }
}
