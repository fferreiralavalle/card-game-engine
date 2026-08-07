using NUnit.Framework;
using RuntimeCardEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICardEntity : MonoBehaviour
{
    public Entity Entity { get; private set; }

    public bool isHidden = false;

    public string presetId = "";

    public TextMeshProUGUI entityName;
    public Image art;
    public TextMeshProUGUI effect;
    public UIPlayCost playCostPrefab;
    public UIResource resourcePrefab;
    public RectTransform playCostContainer;
    public RectTransform attributesContainer;

    public List<UICardResourceItem> specificResources = new();

    public Action<UICardEntity> onDrag;
    public Action<UICardEntity> onDrop;

    public UICardEntity Initiate(Entity entity)
    {
        Entity = entity;
        if (entityName) entityName.text = entity.entityName;
        if (effect) effect.text = entity.runtimeCardEffects.EntityData.effect;

        UpdateCosts();
        InitializeAttributes();

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
            {
                Func<Task> animationTask = async () =>
                {
                    await UpdateAttributes();
                };
                VisualSequencer.Instance.EnqueueAnimation(animationTask);
            }
        };
        game.AddTrigger(onPropertyChange);

        Trigger onBuffEnd = new OnEntityRemoveResourceMod(Entity.runtimeId);
        onBuffEnd.onTrigger += (ev, trigger) =>
        {
            Func<Task> animationTask = async () =>
            {
                await UpdateAttributes();
            };
            VisualSequencer.Instance.EnqueueAnimation(animationTask);
        };
        game.AddTrigger(onBuffEnd);
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

    public void InitializeAttributes()
    {
        EntityTypeData etd = EntityTypeManager.Instance.GetEntityType(Entity.GetCardTypes().First());
        foreach (UICardResourceItem resourceItem in specificResources)
        {
            EntityTypeResource etr = etd.entityTypeResources.Find(etr => etr.resourceId == resourceItem.resourceId);
            Resource resource = Entity.GetProperties().Find(res => res.resourceId == resourceItem.resourceId);
            if (resource != null && etr != null && !etr.hideInEntityUi)
            {
                resourceItem.uIResource.gameObject.SetActive(true);
                resourceItem.uIResource.Initiate(resource);
            }
            else
            {
                resourceItem.uIResource.gameObject.SetActive(false);
            }
        }
        foreach (Resource r in Entity.GetProperties())
        {
            UICardResourceItem resource = specificResources.Find(res => res.resourceId == r.resourceId);
            if (resource == null)
            {
                EntityTypeResource rd = etd.entityTypeResources.Find(etr => etr.resourceId == r.resourceId);
                if (!rd.hideInEntityUi)
                    Instantiate(resourcePrefab, attributesContainer).Initiate(r);
            }
        }
    }

    public async Task UpdateAttributes()
    {
        List<Task> tasks = new List<Task>();

        foreach (Transform resourceItem in attributesContainer)
        {
            UIResource uiResource = resourceItem.GetComponent<UIResource>();
            if (uiResource)
            {
                // Start the task immediately without awaiting it inside the loop
                tasks.Add(uiResource.UpdateResource());
            }
        }

        // Await all tasks concurrently in parallel
        await Task.WhenAll(tasks);
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
