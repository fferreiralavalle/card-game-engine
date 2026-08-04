using Newtonsoft.Json;
using RuntimeNodeEditor;
using System;
using System.Collections.Generic;
/// <summary>
/// This is what is the base data of an entity. It's saved as a Json
/// </summary>
[Serializable]
public class EntityData
{
    /// <summary>
    /// Represents this entity data ID, not unique for runtime
    /// </summary>
    public string entityId;
    public string name;
    public string artPath;
    /// <summary>
    /// This can represent Attack, Health, Mana for players, Victory Points, etc
    /// </summary>
    public Dictionary<string, Resource> resources = new Dictionary<string, Resource>();
    /// <summary>
    /// Entities can have multiple ways of playing for them
    /// </summary>
    public List<PlayCost> costs = new List<PlayCost>();
    public List<string> cardTypeIds = new List<string>();
    public string effect;

    public GraphData graphData;
}
