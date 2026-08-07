using System;
using System.Collections.Generic;

namespace RuntimeCardEngine
{
    [Serializable]
    public class NodeTemplate
    {
        public string nodeID;         // e.g., "action_deal_damage"
        public string nodeName;       // e.g., "Deal Damage"
        public string category;       // e.g., "Actions"
        public string headerColorHex; // e.g., "#FF4D4D" (Red for action, Green for trigger)

        public List<PortDefinition> inputs = new List<PortDefinition>();
        public List<PortDefinition> outputs = new List<PortDefinition>();
        public List<FieldDefinition> fields = new List<FieldDefinition>();
    }

    [Serializable]
    public class PortDefinition
    {
        public string portName;         // e.g., "ExecuteIn", "DamageValue"
        public string portType;         // e.g., "Flow", "Integer", "Entity"
        /// <summary>
        /// Used for identification in events and LUA scripts
        /// </summary>
        public string id;
        /// <summary>
        /// Fill with the custom you want the flow port to execute. Leave Empty for default Execute function. 
        /// </summary>
        public string function;
        // public bool isOutput;           // unused
    }

    [Serializable]
    public class FieldDefinition
    {
        public string fieldName;     // e.g., "DamageAmount"
        public string fieldType;     // e.g., "IntegerField", "DropdownField", "ToggleField"
        public string defaultValue;  // Default fallback value
        public string id;
        public List<string> options; // Only used if fieldType is "DropdownField"
        public bool allowMultiple = false; // Only used if fieldType is "DropdownField"
    }
}