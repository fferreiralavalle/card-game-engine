using MoonSharp.Interpreter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[MoonSharpUserData]
[Serializable]
public class Resource
{
    public string resourceId;
    public int initialAmount = 0;
    [JsonProperty] protected int amount = 0;
    [JsonProperty] protected int maxAmount = 0;
    protected List<ResourceMod> modifications = new List<ResourceMod>();

    public Resource() { }

    public Resource(string resourceId, int amount)
    {
        this.resourceId = resourceId;
        this.amount = amount;
        initialAmount = amount;
        maxAmount = amount;
    }

    public Resource(string resourceId, int amount, int maxAmount)
    {
        this.resourceId = resourceId;
        this.amount = amount;
        this.maxAmount = maxAmount;
        initialAmount = amount;
    }

    public void Modify(ResourceMod mod)
    {
        int index = 0;
        while (index < modifications.Count)
        {
            if (mod.priority <= modifications[index].priority)
            {
                break;
            }
            index++;
        }
        modifications.Insert(index,mod);
    }

    public ResourceMod RemoveModification(string resourceModId)
    {
        ResourceMod mod = modifications.Find((mod) => mod.resourceModId == resourceModId);
        if (mod != null)
        {
            modifications.Remove(mod);
            return mod;
        }
        return null;
    }

    public int GetAmount()
    {
        int modAmount = amount;
        foreach(ResourceMod mod in modifications)
        {
            if (mod.changeType == ChangeType.ADD)
            {
                modAmount += mod.amount;
            }
            else
            {
                modAmount = mod.amount;
            }
        }
        return modAmount;
    }

    public int GetMaxAmount()
    {
        int modMaxAmount = maxAmount;
        int modAmount = GetAmount();
        foreach (ResourceMod mod in modifications)
        {
            if (mod.changeType == ChangeType.ADD)
            {
                modMaxAmount = (int)MathF.Min(maxAmount + mod.maxAmount, modAmount);
            }
            else
            {
                modMaxAmount = (int)MathF.Min(maxAmount + mod.maxAmount, modAmount);
            }
            modMaxAmount = (int)MathF.Min(maxAmount + maxAmount, amount);
        }
        return modMaxAmount;

    }
}
