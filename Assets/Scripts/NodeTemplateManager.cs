using Newtonsoft.Json;
using RuntimeCardEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NodeTemplateManager : MonoBehaviour
{
    public static NodeTemplateManager Instance;

    public string templatesPath = "NodeTemplates";

    protected Dictionary<string, NodeTemplate> nodeTemplates = new Dictionary<string, NodeTemplate>();

    private void Awake()
    {
        Instance = this;
        LoadTemplatesFromFolder();
    }

    private void LoadTemplatesFromFolder()
    {
        string templatesFolder = Path.Combine(Application.streamingAssetsPath, templatesPath);
        if (!Directory.Exists(templatesFolder)) Directory.CreateDirectory(templatesFolder);

        string[] filePaths = Directory.GetFiles(templatesFolder, "*.json", SearchOption.AllDirectories);
        foreach (var path in filePaths)
        {
            string jsonText = File.ReadAllText(path);
            NodeTemplate template = JsonConvert.DeserializeObject<NodeTemplate>(jsonText);
            if (template != null)
            {
                nodeTemplates.Add(template.nodeID, template);
            }
        }
    }

    public Dictionary<string, NodeTemplate> GetNodeTemplates()
    {
        return nodeTemplates;
    }

    public NodeTemplate GetNodeTemplate(string nodeID)
    {
        return nodeTemplates[nodeID];
    }
}
