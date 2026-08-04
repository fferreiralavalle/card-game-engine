using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIImagePreview : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI imageName;

    public UIImagePreview Initiate(string artPath, string name)
    {
        imageName.text = name;
        LoadArt(artPath);
        return this;
    }


    public async Task LoadArt(string artPath)
    {
        Sprite art = await AssetsManager.Instance.GetSpriteAsync(artPath);
        image.sprite = art;
    }
}
