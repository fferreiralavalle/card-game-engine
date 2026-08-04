using System;

[Serializable]
public class EntityTypeResource
{
    public string resourceId = "";
    public int initialValue = 1;
    public bool hideInEntityUi = false;

    public EntityTypeResource (string resourceId, int initialValue)
    {
        this.resourceId = resourceId;
        this.initialValue = initialValue;
    }
}
