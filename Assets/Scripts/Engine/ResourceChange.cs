using MoonSharp.Interpreter;
using System;
using UnityEngine;

[Serializable]
[MoonSharpUserData]
public class ResourceChange
{
    public string resourceId;
    public ResourceMod resourceMod;

    public ResourceChange() { }

    public ResourceChange(string resourceId, ResourceMod resourceMod)
    {
        this.resourceId = resourceId;
        this.resourceMod = resourceMod;
    }
}
