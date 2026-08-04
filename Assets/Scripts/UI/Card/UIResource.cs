using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResource : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI value;

    public Resource resource;

    public UIResource Initiate(Resource resource)
    {
        this.resource = resource;
        value.text = resource.GetAmount().ToString();
        ResourceData resourceData = ResourceManager.Instance.GetResource(resource.resourceId);
        LoadIcon(resourceData.iconPath);
        return this;
    }

    public async void LoadIcon(string path)
    {
        Sprite icon = await AssetsManager.Instance.GetSpriteAsync(path);
        if (icon != null && this.icon)
        {
            this.icon.sprite = icon;
        }
    }
}
