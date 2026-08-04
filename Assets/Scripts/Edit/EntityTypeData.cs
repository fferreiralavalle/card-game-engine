using System;
using System.Collections.Generic;

[Serializable]
public class EntityTypeData
{
    public string entityTypeId;
    public string name;
    public string afterPlayZone;
    public List<EntityTypeResource> entityTypeResources = new List<EntityTypeResource>();
}
