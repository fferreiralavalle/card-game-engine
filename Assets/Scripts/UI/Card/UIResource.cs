using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // 1. Added DOTween namespace

public class UIResource : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI value;
    public TextMeshProUGUI maxValue;

    public Resource resource;
    public Color overInitialColor = Color.darkGreen;
    public Color underMaxColor = Color.darkRed;

    public Vector2 changeGrowScale = new Vector2(1.3f, 1.3f);

    protected Color initialColor;
    protected int oldAmount = 0;
    protected int oldMaxAmount = 0;

    public UIResource Initiate(Resource resource)
    {
        initialColor = value.color;
        oldAmount = resource.GetAmount();
        oldMaxAmount = resource.GetMaxAmount();
        this.resource = resource;
        UpdateResource();
        ResourceData resourceData = ResourceManager.Instance.GetResource(resource.resourceId);
        LoadIcon(resourceData.iconPath);
        return this;
    }

    public async Task UpdateResource()
    {
        if (resource != null)
        {
            int amount = resource.GetAmount();
            int initialAmount = resource.initialAmount;
            int maxAmount = resource.GetMaxAmount();

            value.text = amount.ToString();

            if (amount > initialAmount)
            {
                value.color = overInitialColor;
            }
            else if (amount < maxAmount)
            {
                value.color = underMaxColor;
            }
            else
            {
                value.color = initialColor;
            }

            if (maxValue != null)
            {
                maxValue.text = maxAmount.ToString();
            }

            // 2. Trigger grow animation if maxAmount changed
            if (maxAmount != oldMaxAmount)
            {
                if (maxValue)
                {
                    maxValue?.transform.DOKill();
                    maxValue.transform.localScale = Vector3.one;
                }

                Sequence seq = DOTween.Sequence();
                // Grow to changeGrowScale size
                seq.Append(value.transform.DOScale(changeGrowScale, 0.3f).SetEase(Ease.OutQuad));
                if (maxValue) seq.Join(maxValue.transform.DOScale(changeGrowScale, 0.3f).SetEase(Ease.OutQuad));

                // Return back to 1.0x size
                seq.Append(value.transform.DOScale(1.0f, 0.15f).SetEase(Ease.InQuad));
                if (maxValue) seq.Join(maxValue.transform.DOScale(1.0f, 0.15f).SetEase(Ease.InQuad));

                await seq.AsyncWaitForCompletion();
            }

            oldAmount = amount;
            oldMaxAmount = maxAmount;
        }
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