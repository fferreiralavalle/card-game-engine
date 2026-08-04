using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;

public class UIEntityEditor : MonoBehaviour
{
    public TMP_InputField entityName;
    public TMP_Dropdown typeDropdown;
    public Button pickArtButton;
    public UIImagePreview artPreview;
    public Transform costContainer;
    public Transform resourceContainer;
    public UIArtPicker artPicker;

    public GameObject dropDownFieldPrefab;
    public GameObject integerFieldPrefab;

    public TMP_InputField effect;

    protected EntityData entity = new EntityData();

    protected List<EntityTypeData> entityTypes = new List<EntityTypeData>();
    protected List<ResourceData> resources = new List<ResourceData>();

    private void Start()
    {
        artPicker.OnPickImage += HandleChooseArt;
        Initialize();
    }

    public EntityData GetEntity()
    {
        return entity;
    }

    public void Initialize()
    {
        EntityData entity = new EntityData();
        entity.cardTypeIds = new List<string>() { "minion" };
        Load(entity);
        typeDropdown.onValueChanged.AddListener(HandleDropDownTypeChange);
        entityName.onValueChanged.AddListener(handleNameChange);
        effect.onValueChanged.AddListener(HandleEffectChanged);
    }

    public void Load(EntityData entity)
    {
        this.entity = entity;

        // Load Resources
        resources = ResourceManager.Instance.GetResources();
        // Load Entity Types
        entityTypes = EntityTypeManager.Instance.GetEntityTypes();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (EntityTypeData entityTypeData in entityTypes)
        {
            options.Add(new TMP_Dropdown.OptionData(entityTypeData.name));
        }
        typeDropdown.ClearOptions();
        typeDropdown.AddOptions(options);
        effect.text = entity.effect;
        HandleChooseArt(entity.artPath);

        HandleTypeChange(entityTypes[0].entityTypeId);
    }

    protected void handleNameChange(string name)
    {
        entity.name = name;

    }

    protected void HandleDropDownTypeChange(int index)
    {
        HandleTypeChange(entityTypes[index].entityTypeId);
    }

    public void HandleTypeChange(string newEntityTypeId)
    {
        entity.cardTypeIds = new List<string> { newEntityTypeId };
        // Remove unused resources
        EntityTypeData newEntityType = entityTypes.Find(et => et.entityTypeId == newEntityTypeId);
        if (newEntityType != null)
        {
            List<string> entityResources = entity.resources.Keys.ToList();
            foreach (string resourceName in entityResources)
            {
                // If current entity resource does not exist in it's entity type, remove it. Otherwise, keep it
                if (newEntityType.entityTypeResources.Find(etr => etr.resourceId == resourceName) == null)
                {
                    entity.resources.Remove(resourceName);
                }
            }
            foreach(EntityTypeResource resource in newEntityType.entityTypeResources)
            {
                // If current entity does have its new type resource, add it
                if (entity.resources.Keys.ToList().Find(resourceId => resourceId == resource.resourceId) == null)
                {
                    entity.resources.Add(resource.resourceId, new Resource(resource.resourceId, resource.initialValue));
                }
            }

            foreach (Transform cost in costContainer.transform)
            {
                Destroy(cost.gameObject);
            }
            foreach (Transform cost in resourceContainer.transform)
            {
                Destroy(cost.gameObject);
            }
            foreach (Resource r in entity.resources.Values.ToList())
            {
                GameObject integerField = Instantiate(integerFieldPrefab, resourceContainer);
                integerField.GetComponentInChildren<TMP_InputField>().text = r.initialAmount.ToString();
                ResourceData rd = ResourceManager.Instance.GetResource(r.resourceId);
                integerField.GetComponentInChildren<TMP_Text>().text = rd.name;
                integerField.GetComponentInChildren<TMP_InputField>()?.onValueChanged.AddListener((newValue => HandleResourceValueChange(rd.resourceId, newValue)));
            }
            if (entity.costs.Count == 0)
            {
                entity.costs.Add(new PlayCost());
            }
            for (int index = 0; index<entity.costs.Count; index++)
            {
                // Makes a copy in a new variable of index at that time
                int i = index;
                PlayCost pc = entity.costs[i];
                foreach (ResourceData rd in resources)
                {
                    GameObject integerField = Instantiate(integerFieldPrefab, costContainer);
                    Resource r = null;
                    if (!pc.costs.ContainsKey(rd.resourceId))
                    {
                        r = new Resource(rd.resourceId, 0);
                    }
                    else
                    {
                        r = pc.costs[rd.resourceId];
                    }
                    integerField.GetComponentInChildren<TMP_InputField>().text = r.initialAmount.ToString();
                    integerField.GetComponentInChildren<TMP_Text>().text = rd.name;
                    integerField.GetComponentInChildren<TMP_InputField>()?.onValueChanged.AddListener(newValue => HandleCostValueChange(i, rd.resourceId, newValue));
                }
            }
        }
        
    }

    public void HandleResourceValueChange(string resourceId, string newValue)
    {
        entity.resources[resourceId] = new Resource(resourceId, int.Parse(newValue));
    }

    public void HandleCostValueChange(int costIndex, string resourceId, string newValue)
    {
        entity.costs[costIndex].costs[resourceId] = new Resource(resourceId, int.Parse(newValue));
    }

    public void HandleEffectChanged(string newEffect)
    {
        entity.effect = newEffect;
    }

    public void HandleOpenArt()
    {
        artPicker.gameObject.SetActive(true);
    }

    public void HandleChooseArt(string newArt)
    {
        entity.artPath = newArt;
        if (string.IsNullOrEmpty(newArt))
        {
            artPreview.gameObject.SetActive(false);
            pickArtButton.gameObject.SetActive(true);
        }
        else
        {
            artPreview.Initiate(newArt, Path.GetFileName(newArt));
            artPreview.gameObject.SetActive(true);
            pickArtButton.gameObject.SetActive(false);
        }
        artPicker.gameObject.SetActive(false);
    }
}
