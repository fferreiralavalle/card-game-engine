using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class EntityTemplateManager : MonoBehaviour
{
    public static EntityTemplateManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Initialize();
    }

    public string folderPath = "CustomCards";
    public Dictionary<string, EntityData> entities = new Dictionary<string, EntityData>();

    public void Initialize()
    {
        entities = LoadEntityTemplates();
    }

    public Dictionary<string, EntityData> LoadEntityTemplates()
    {
        string finalFolderPath = Path.Combine(Application.streamingAssetsPath, folderPath);
        if (!Directory.Exists(finalFolderPath)) Directory.CreateDirectory(finalFolderPath);

        // Load all TextAssets in the specified folder
        string[] filePaths = Directory.GetFiles(finalFolderPath, "*.json");
        Dictionary<string, EntityData> loadedEntities = new Dictionary<string, EntityData>();
        print($"Loading Entities from: {finalFolderPath})");
        foreach (string path in filePaths)
        {
            string jsonText = File.ReadAllText(path);
            // Parse the JSON text into C# class
            EntityData item = JsonConvert.DeserializeObject<EntityData>(jsonText);
            if (item != null && item.entityId != null)
            {
                loadedEntities.Add(item.entityId, item);
                print($"Loaded Entity: {item.entityId} ({item.name})");
            }
        }
        print($"Loaded Entities: {loadedEntities.Values.Count}");
        return loadedEntities;
    }

    public List<EntityData> GetEntityTemplates()
    {
        return entities.Values.ToList();
    }

    public EntityData GetEntity(string entityId)
    {
        return entities[entityId];
    }
}
