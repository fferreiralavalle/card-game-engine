using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RuntimeNodeEditor;
using Newtonsoft.Json;
using System.Linq;

namespace RuntimeCardEngine
{
    public class CardCreatorEditor : NodeEditor
    {
        [Header("Dynamic Node Configuration")]
        [Tooltip("The path to the single generic prefab inside a Resources folder, e.g., 'Nodes/DynamicCardNode'")]
        public string genericNodePrefabResourcePath = "Nodes/DynamicCardNode";
        public UIEntityEditor entityEditor;

        public Color errorColor = Color.red;

        private string _saveFolder;
        private List<NodeTemplate> _loadedTemplates = new List<NodeTemplate>();

        public static CardCreatorEditor Instance;

        private void Awake()
        {
            Instance = this;
        }

        public override void StartEditor(NodeGraph graph)
        {
            base.StartEditor(graph);

            // Establish our standard paths
            _saveFolder = Path.Combine(Application.streamingAssetsPath, "CustomCards");
            if (!Directory.Exists(_saveFolder)) Directory.CreateDirectory(_saveFolder);

            // 1. Hook up the basic framework events
            Events.OnGraphPointerClickEvent += OnGraphPointerClick;
            Events.OnNodePointerClickEvent += OnNodePointerClick;
            Events.OnConnectionPointerClickEvent += OnNodeConnectionPointerClick;
            Events.OnSocketConnect += OnConnect;

            // Set canvas limits matching your example layout configuration
            Graph.SetSize(Vector2.one * 20000);
        }

        private void OnConnect(SocketInput input, SocketOutput output)
        {
            // Give card flow links a distinct color like green
            DynamicCardNode inputNode = input.OwnerNode as DynamicCardNode;
            DynamicCardNode outPutNode = output.OwnerNode as DynamicCardNode;

            string inputPortType = inputNode.GetNodeTemplate().inputs.Find(templateInp => inputNode.GenerateSocketId(templateInp) == input.socketId).portType;
            string outputPortType = outPutNode.GetNodeTemplate().outputs.Find(inp => outPutNode.GenerateSocketId(inp) == output.socketId).portType;
            
            if (inputPortType != outputPortType)
            {
                Graph.drawer.SetConnectionColor(output.connection.connId, errorColor);
            }
        }

        private void OnGraphPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
            {
                if (eventData.button == PointerEventData.InputButton.Left) CloseContextMenu();
                return;
            }

            // Create our builder instance
            var builder = new ContextMenuBuilder();

            // 1. DYNAMICALLY inject every loaded JSON file option into the menu array!
            _loadedTemplates = NodeTemplateManager.Instance.GetNodeTemplates().Values.ToList();
            foreach (var template in _loadedTemplates)
            {
                string pathInMenu = $"Add Node/{template.category}/{template.nodeName}";

                // We use a lambda to capture the specific template closure securely
                builder.Add(pathInMenu, () => CreateDynamicNode(template));
            }

            builder.Add("Card File/Save Card", () => SaveEntity(_saveFolder));
            builder.Add("Card File/Load Card", () => LoadEntity(_saveFolder));

            // Build and draw it
            SetContextMenu(builder.Build());
            DisplayContextMenu();
        }

        private void CreateDynamicNode(NodeTemplate template)
        {
            CloseContextMenu();

            // Instantiate our generic view container component through the graph framework
            Node baseNode = ((UIGraph)Graph).CreateWithInstance(genericNodePrefabResourcePath);

            // Feed our text definitions directly into our customized visual layout logic
            if (baseNode is DynamicCardNode dynamicNode)
            {
                dynamicNode.PopulateAndSetup(template);
                dynamicNode.PopulateFields(template);
            }
        }

        public void HandleSave()
        {
            SaveEntity(_saveFolder);
        }

        // --- Standard Utility Options matching your exact example logic ---

        private void SaveEntity(string savePath)
        {
            CloseContextMenu();
            GraphData graphData = Graph.Export();
            EntityData entity = entityEditor.GetEntity();
            entity.graphData = graphData;
            entity.entityId = entity.entityId ?? entity.name;
            string fileName = $"{entity.name}.json";
            string defaultSavePath = Path.Combine(_saveFolder, fileName);
            string entityData = JsonConvert.SerializeObject(entity, Formatting.Indented);
            File.WriteAllText(defaultSavePath, entityData);
            Debug.Log($"Card layout saved successfully to: {savePath} as {fileName}");
        }

        private void LoadEntity(string savePath)
        {
            EntityData entity = entityEditor.GetEntity();
            string loadPath = Path.Combine(_saveFolder, $"{entity.name}.json");
            CloseContextMenu();
            if (File.Exists(loadPath))
            {
                Graph.Clear();
                var file = File.ReadAllText(loadPath);
                var entityData = JsonConvert.DeserializeObject<EntityData>(file);
                entityEditor.Load(entityData);
                Graph.LoadGraph(entityData.graphData);
                Debug.Log($"Card layout loaded successfully from: {savePath}");
            }
            else
            {
                Debug.LogError($"Specified file {loadPath} not exist.");
            }
        }

        private void OnNodePointerClick(Node node, PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                var ctx = new ContextMenuBuilder()
                .Add("Duplicate Node", () => { Graph.Duplicate(node); CloseContextMenu(); })
                .Add("Clear Connections", () => { Graph.ClearConnectionsOf(node); CloseContextMenu(); })
                .Add("Delete Node", () => { Graph.Delete(node); CloseContextMenu(); })
                .Build();

                SetContextMenu(ctx);
                DisplayContextMenu();
            }
        }

        private void OnNodeConnectionPointerClick(string connId, PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                var ctx = new ContextMenuBuilder()
                .Add("Disconnect Wire", () => { Graph.Disconnect(connId); CloseContextMenu(); })
                .Build();

                SetContextMenu(ctx);
                DisplayContextMenu();
            }
        }
    }
}