using MoonSharp.Interpreter;
using Newtonsoft.Json;
using RuntimeNodeEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeCardEngine
{
    [MoonSharpUserData]
    public class RuntimeCardEffectsData
    {
        public EntityData EntityData { get; set; }
        public List<RuntimeNode> nodes = new List<RuntimeNode>();
        public List<RuntimeConnection> connections = new List<RuntimeConnection>();

        public RuntimeCardEffectsData() { }
        public RuntimeCardEffectsData(EntityData entityData)
        {
            EntityData = entityData;
            foreach (NodeData node in entityData.graphData.nodes)
            {
                string templateId = node.values.First(val => val.key == "templateID").value;
                NodeTemplate nt = NodeTemplateManager.Instance.GetNodeTemplate(templateId);
                nodes.Add(new RuntimeNode(node, nt));
            }
            foreach(var connection in entityData.graphData.connections)
            {
                RuntimeConnection runtimeConnection = new RuntimeConnection();
                runtimeConnection.fromPortId = connection.outputSocketId;
                runtimeConnection.toPortId = connection.inputSocketId;
                foreach (NodeData node in entityData.graphData.nodes)
                {
                    if (node.inputSocketIds.Contains(connection.inputSocketId))
                    {
                        runtimeConnection.toNodeId = node.id;
                    }
                    else if (node.outputSocketIds.Contains(connection.outputSocketId))
                    {
                        runtimeConnection.fromNodeId = node.id;
                    }
                }
                connections.Add(runtimeConnection);
            }
        }
    }

    [MoonSharpUserData]
    public class RuntimeNode
    {
        public string id;          // Unique GUID representing this node instance
        public string nodeType;    // Matches the JSON template ID (e.g., "action_deal_damage")
        
        public Dictionary<string, object> fieldValues = new Dictionary<string, object>();
        public List<RuntimeKeyValue> inputs = new List<RuntimeKeyValue>();
        public List<RuntimeKeyValue> outputs = new List<RuntimeKeyValue>();

        public List<string> inputSocketIds;
        public List<string> outputSocketIds;

        public NodeTemplate template;
        public Script luaScript;

        public int timesExectued = 0;

        /// <summary>
        /// string is outputID (ex: "onDone", "onTry")
        /// </summary>
        public Action<string> triggerFlow;

        public RuntimeNode() { }
        public RuntimeNode(NodeData nodeData, NodeTemplate nodeTemplate)
        {
            inputSocketIds = nodeData.inputSocketIds.ToList();
            outputSocketIds = nodeData.outputSocketIds.ToList();

            id = nodeData.id;
            nodeType = nodeData.values.ToList().Find(v => v.key == "templateID").value;
            template = nodeTemplate;
            int i = 0;
            foreach(var field in nodeData.values.ToList().FindAll(v => v.key != "templateID").ToList()){
                if (nodeTemplate.fields[i].allowMultiple)
                {
                    FieldDefinition fieldDefinition = nodeTemplate.fields[i];
                    fieldValues[field.key] = JsonConvert.DeserializeObject<List<object>>(field.value);
                }
                else
                {
                    fieldValues[field.key] = field.value;
                }
                i++;
            }
            foreach(var input in nodeTemplate.inputs)
            {
                RuntimeKeyValue serializedValue = new RuntimeKeyValue();
                serializedValue.key = input.id ?? input.portName;
                serializedValue.value = null;
                inputs.Add(serializedValue);
            }
            foreach (var output in nodeTemplate.outputs)
            {
                RuntimeKeyValue serializedValue = new RuntimeKeyValue();
                serializedValue.key = output.id ?? output.portName;
                serializedValue.value = null;
                outputs.Add(serializedValue);
            }
        }

        public T GetOutputValue<T>(string IdToconnectionOutput)
        {
            int index = outputSocketIds.FindIndex(portId => portId == IdToconnectionOutput);
            if (index >= 0)
            {
                object outputValue = outputs[index].value;
                return (T)(outputValue ?? default(T));
            }
            return default(T);
        }

        public string GetOutputSocketIdById(string outputId)
        {
            int index = outputs.FindIndex(input => input.key == outputId);
            if (index >= 0)
            {
                return outputSocketIds[index];
            }
            return null;
        }

        public void SetOutputValue(string outputId, object newVal)
        {
            // 1. Unwrap DynValue if MoonSharp wrapped it
            if (newVal is DynValue dynVal)
            {
                newVal = dynVal.Type == DataType.Table ? dynVal.Table : dynVal.ToObject();
            }

            // 2. Convert raw Lua Tables into clean C# Lists of underlying objects
            if (newVal is Table luaTable)
            {
                List<object> csharpList = new List<object>();

                foreach (var pair in luaTable.Pairs)
                {
                    // pair.Value.ToObject() strips the Lua Script reference and extracts 
                    // the pure C# object (e.g., Zone, Entity, string, etc.)
                    object obj = pair.Value.ToObject();
                    if (obj != null)
                    {
                        csharpList.Add(obj);
                    }
                }

                newVal = csharpList;
            }

            // 3. Store the clean, script-independent value
            RuntimeKeyValue val = outputs.Find(output => output.key == outputId);
            if (val == null)
            {
                val = new RuntimeKeyValue();
                val.key = outputId;
                outputs.Add(val);
            }
            val.value = newVal;
        }

        public T GetInputValue<T>(string IdToconnectionInput)
        {
            int index = inputSocketIds.FindIndex(portId => portId == IdToconnectionInput);
            if (index >= 0)
            {
                object inputValue = inputs[index].value;
                return (T)(inputValue ?? default(T));
            }
            return default(T);
        }

        public string GetInputSocketIdById(string inputId)
        {
            int index = inputs.FindIndex(input => input.key == inputId);
            if (index >= 0)
            {
                return inputSocketIds[index];
            }
            return null;
        }

        public void SetInputValue(string inputId, object newVal)
        {
            RuntimeKeyValue val = inputs.Find(input => input.key == inputId);
            if (val == null)
            {
                val = new RuntimeKeyValue();
                val.key = inputId;
                val.value = newVal;
            }
            val.value = newVal;
        }
    }

    [MoonSharpUserData]
    public class RuntimeKeyValue
    {
        public string key;
        public object value;
    }

    // TODO: Check this variables, fromPortName should be fromPortId, same with toPortName
    [MoonSharpUserData]
    public class RuntimeConnection
    {
        public string fromNodeId;
        public string fromPortId;
        public string toNodeId;
        public string toPortId;
    }
}