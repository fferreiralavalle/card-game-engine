using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class EntityTypeManager : MonoBehaviour
{
    public static EntityTypeManager Instance;

    public string folderPath = "EntityTypes";
    public Dictionary<string, EntityTypeData> entities = new Dictionary<string, EntityTypeData>();

    private void Awake()
    {
        Instance = this;
        Initialize();
    }

    public void Initialize()
    {
        entities = GetResourcesAsDictionary();
    }

    public Dictionary<string, EntityTypeData> GetResourcesAsDictionary()
    {
        string finalFolderPath = Path.Combine(Application.streamingAssetsPath, folderPath);
        if (!Directory.Exists(finalFolderPath)) Directory.CreateDirectory(finalFolderPath);

        // Load all TextAssets in the specified folder
        string[] filePaths = Directory.GetFiles(finalFolderPath, "*.json");
        Dictionary<string, EntityTypeData> entityTypes = new Dictionary<string, EntityTypeData>();
        print($"Loading Entity types from: {finalFolderPath})");
        foreach (string path in filePaths)
        {
            string jsonText = File.ReadAllText(path);
            // Parse the JSON text into C# class
            EntityTypeData item = JsonUtility.FromJson<EntityTypeData>(jsonText);
            entityTypes.Add(item.entityTypeId, item);
            print($"Loaded Entity: {item.entityTypeId} ({item.name})");
        }
        print($"Loaded Entities: {entityTypes.Values.Count}");
        return entityTypes;
    }

    public List<EntityTypeData> GetEntityTypes()
    {
        return entities.Values.ToList();
    }

    public EntityTypeData GetEntityType(string entityTypeId)
    {
        return entities[entityTypeId];
    }
}
