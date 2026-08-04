using UnityEngine;
using TMPro;
using System.Collections.Generic;
using RuntimeNodeEditor; // The cemuka framework namespace
using UnityEngine.UI;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RuntimeCardEngine
{
    public class DynamicCardNode : Node
    {
        public Image headerBackground;
        [Header("Prefab References")]
        public GameObject inputSocketPrefab;   // Standard prefab with SocketInput attached
        public GameObject outputSocketPrefab;  // Standard prefab with SocketOutput attached
        public Transform leftContainer;        // Vertical layout group for inputs
        public Transform rightContainer;       // Vertical layout group for outputs

        [Header("Content Prefabs")]
        public GameObject integerFieldPrefab;   // Standard UI containing TMP_InputField configured for numbers
        public GameObject dropdownFieldPrefab;  // Standard UI containing TMP_Dropdown
        public GameObject toggleFieldPrefab;    // Standard UI containing a Toggle
        public Transform contentContainer;      // Vertical layout panel in the center of the node

        // Keep a dictionary of spawned fields so we can easily read their values when saving!
        private Dictionary<string, GameObject> _spawnedFields = new Dictionary<string, GameObject>();

        // Tracks runtime connection tracking variables
        private List<IOutput> _incomingOutputs = new List<IOutput>();

        protected NodeTemplate _myTemplate;

        public NodeTemplate GetNodeTemplate() { return _myTemplate; }

        public void PopulateAndSetup(NodeTemplate template)
        {
            string nodeId = ID;
            _myTemplate = template;

            SetHeader(template.nodeName);
            if (headerBackground)
            {
                Color headerColor;
                ColorUtility.TryParseHtmlString(template.headerColorHex, out headerColor);
                headerBackground.color = headerColor;
            }

            // Generate and Register Ports dynamically from your JSON data block
            foreach (var portDef in template.inputs)
            {
                // Create a unique, deterministic ID for this port instance
                // Format: "nodeID_portName" (e.g., "cc717ef3..._DamageAmount")
                string assignedSocketId = GenerateSocketId(portDef);


                // 1. Spawn Input Container Object
                GameObject obj = Instantiate(inputSocketPrefab, leftContainer);
                obj.GetComponentInChildren<TMP_Text>().text = $"{portDef.portName} ({portDef.portType})";

                // 2. Extract and Register Component
                SocketInput socket = obj.GetComponent<SocketInput>();
                socket.name = $"{portDef.portName} ({portDef.portType})"; ;

                // Explicitly set the framework's internal connection ID tracking variable
                // Depending on your fork version, this is either socket.Initialize(id), socket.id = id, or socket.SetID(id)
                socket.socketId = assignedSocketId;

                Register(socket);
            }
            foreach (var portDef in template.outputs)
            {
                string assignedSocketId = GenerateSocketId(portDef);

                // 1. Spawn Output Container Object
                GameObject obj = Instantiate(outputSocketPrefab, rightContainer);
                obj.GetComponentInChildren<TMP_Text>().text = $"{portDef.portName} ({portDef.portType})";

                // 2. Extract and Register Component
                SocketOutput socket = obj.GetComponent<SocketOutput>();

                // CRITICAL: We name the connection path so JSON savers know what wire goes where
                socket.name = $"{portDef.portName} ({portDef.portType})";

                // Explicitly set the framework's internal connection ID tracking variable
                // Depending on your fork version, this is either socket.Initialize(id), socket.id = id, or socket.SetID(id)
                socket.socketId = assignedSocketId.ToString();
                socket.SetValue(template);

                Register(socket);
            }

            // Hook up listeners for runtime evaluation matching the example
            OnConnectionEvent += OnConnection;
            OnDisconnectEvent += OnDisconnect;
        }

        public string GenerateSocketId(PortDefinition portDef)
        {
            string assignedSocketId = $"{ID}_{portDef.portName}";
            return assignedSocketId;
        }

        public void OnConnection(SocketInput input, IOutput output)
        {
            _incomingOutputs.Add(output);
            // Engine execution paths fire downstream logic nodes here
        }

        public void OnDisconnect(SocketInput input, IOutput output)
        {
            _incomingOutputs.Remove(output);
        }

        public void PopulateFields(NodeTemplate template)
        {
            foreach (var fieldDef in template.fields)
            {
                GameObject fieldObj = null;

                switch (fieldDef.fieldType)
                {
                    case "IntegerField":
                        fieldObj = Instantiate(integerFieldPrefab, contentContainer);
                        var inputField = fieldObj.GetComponentInChildren<TMP_InputField>();
                        inputField.text = fieldDef.defaultValue;
                        break;

                    case "DropdownField":
                        fieldObj = Instantiate(dropdownFieldPrefab, contentContainer);
                        var dropdown = fieldObj.GetComponentInChildren<TMP_Dropdown>();
                        dropdown.ClearOptions();
                        dropdown.AddOptions(fieldDef.options);
                        dropdown.MultiSelect = fieldDef.allowMultiple;

                        // Set default if found
                        int defaultIndex = fieldDef.options.IndexOf(fieldDef.defaultValue);
                        if (defaultIndex != -1) dropdown.value = defaultIndex;
                        break;

                    case "ToggleField":
                        fieldObj = Instantiate(toggleFieldPrefab, contentContainer);
                        var toggle = fieldObj.GetComponentInChildren<Toggle>();
                        toggle.isOn = bool.Parse(fieldDef.defaultValue);
                        break;
                }

                if (fieldObj != null)
                {
                    // Label the field so the user knows what they are editing
                    var label = fieldObj.GetComponentInChildren<TMP_Text>();
                    if (label != null) label.text = fieldDef.fieldName;

                    // Save a reference to this UI row using its unique fieldName
                    _spawnedFields.Add(fieldDef.id, fieldObj);
                }
            }
        }

        public override void OnSerialize(Serializer serializer)
        {
            // 1. Let the framework save standard port wires automatically
            base.OnSerialize(serializer);

            // 2. Save the template ID so we know how to rebuild this node on load
            if (_myTemplate != null)
            {
                serializer.Add("templateID", _myTemplate.nodeID);
            }

            // 3. Extract values from our dynamic UI elements to save with the card file
            foreach (var kvp in _spawnedFields)
            {
                string fieldName = kvp.Key;
                GameObject fieldObj = kvp.Value;

                // Check which component is inside this field row and extract its text
                var input = fieldObj.GetComponentInChildren<TMP_InputField>();
                if (input != null)
                {
                    serializer.Add(fieldName, input.text);
                    continue;
                }

                var dropdown = fieldObj.GetComponentInChildren<TMP_Dropdown>();
                if (dropdown != null)
                {
                    // Save the selected text option string
                    if (dropdown.MultiSelect)
                    {
                        List<int> values = dropdown.GetSelectedIndexes();
                        List<string> strings = values.Select(index => dropdown.options[index].text).ToList();
                        string stringArray = JsonConvert.SerializeObject(strings, Formatting.Indented);
                        serializer.Add(fieldName, stringArray);
                    }
                    else
                    {
                        serializer.Add(fieldName, dropdown.options[dropdown.value].text);
                    }
                    continue;
                }

                var toggle = fieldObj.GetComponentInChildren<Toggle>();
                if (toggle != null)
                {
                    serializer.Add(fieldName, toggle.isOn.ToString());
                    continue;
                }
            }
        }

        public override void OnDeserialize(Serializer serializer)
        {
            // 1. Grab the template ID from the file
            string savedTemplateId = serializer.Get("templateID");

            // 2. Fetch the original template blueprint from our Editor registry
            // (Assuming CardCreatorEditor keeps a public static reference or lookup method)
            NodeTemplate template = NodeTemplateManager.Instance.GetNodeTemplate(savedTemplateId);

            if (template != null)
            {
                // 3. Re-draw all the visual sockets and register them with the exact same IDs
                PopulateAndSetup(template);
                PopulateFields(template);

                // 4. Now that the UI fields physically exist again, populate their values from the file!
                foreach (var kvp in _spawnedFields)
                {
                    string fieldName = kvp.Key;
                    GameObject fieldObj = kvp.Value;

                    // Check if the file contains a saved value for this field
                    string savedValue = serializer.Get(fieldName);
                    if (string.IsNullOrEmpty(savedValue)) continue;

                    var input = fieldObj.GetComponentInChildren<TMP_InputField>();
                    if (input != null) input.text = savedValue;

                    var dropdown = fieldObj.GetComponentInChildren<TMP_Dropdown>();
                    if (dropdown != null)
                    {
                        if (dropdown.MultiSelect)
                        {
                            List<string> listValues = JsonConvert.DeserializeObject<List<string>>(savedValue);
                            dropdown.SetSelectedOptionsByText(listValues);
                        }
                        else
                        {
                            int index = dropdown.options.FindIndex(o => o.text == savedValue);
                            if (index != -1) dropdown.value = index;
                        }
                    }

                    var toggle = fieldObj.GetComponentInChildren<Toggle>();
                    if (toggle != null) toggle.isOn = bool.Parse(savedValue);
                }
            }
            else
            {
                Debug.LogError($"Could not find template blueprint '{savedTemplateId}' to restore this node!");
            }

            // 5. Let the base framework reconnect all the green wires using the restored socket IDs
            base.OnDeserialize(serializer);
        }
    }
}