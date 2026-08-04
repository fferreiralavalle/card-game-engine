using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIArtPicker : MonoBehaviour
{
    public static UIArtPicker Instance { get; private set; }

    public UIImagePreview imagePreviewPrefab;
    public Transform content;


    public Action<string> OnPickImage;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Initiate();
    }

    public UIArtPicker Initiate()
    {
        List<string> arts = AssetsManager.Instance.GetAllImages("Art");

        foreach (string art in arts)
        {
            UIImagePreview ip = Instantiate(imagePreviewPrefab, content).Initiate(art, Path.GetFileName(art));
            string result = "";
            string spritesPath = AssetsManager.Instance.GetAssetsSpritesFullPath();
            if (art.StartsWith(spritesPath, StringComparison.OrdinalIgnoreCase))
            {
                // Extract the remainder and remove leading separators
                result = art.Substring(spritesPath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            ip.GetComponent<Button>().onClick.AddListener(() => { HandleSelect(result); });
        }

        return this;
    }

    public void HandleSelect(string artRelativePath)
    {
        OnPickImage?.Invoke(artRelativePath);
    }
}
