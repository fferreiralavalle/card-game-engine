using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    public string folderPath = "StreamingAssets/Resources";
    public Dictionary<string, ResourceData> resources = new Dictionary<string, ResourceData>();

    private void Awake()
    {
        Instance = this;
        Initialize();
    }

    public void Initialize()
    {
        resources = GetResourcesAsDictionary();
    }

    public Dictionary<string, ResourceData> GetResourcesAsDictionary()
    {
        string finalFolderPath = Path.Combine(Application.streamingAssetsPath, folderPath);
        if (!Directory.Exists(finalFolderPath)) Directory.CreateDirectory(finalFolderPath);
        // Load all TextAssets in the specified folder
        string[] filePaths = Directory.GetFiles(finalFolderPath, "*.json");
        Dictionary<string, ResourceData> resources = new Dictionary<string, ResourceData>();
        print($"Loading Resources from: {finalFolderPath})");
        foreach (string path in filePaths)
        {
            string jsonText = File.ReadAllText(path);
            // Parse the JSON text into C# class
            ResourceData item = JsonUtility.FromJson<ResourceData>(jsonText);
            resources.Add(item.resourceId, item);
            print($"Loaded Resource: {item.resourceId} ({item.name})");
        }
        print($"Loaded Resources: {resources.Values.Count}");
        return resources;
    }

    public List<ResourceData> GetResources()
    {
        return resources.Values.ToList();
    }

    public ResourceData GetResource(string resourceId)
    {
        return resources[resourceId];
    }
}
