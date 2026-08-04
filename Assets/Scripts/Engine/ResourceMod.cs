using MoonSharp.Interpreter;
using System;
[Serializable]
[MoonSharpUserData]
public class ResourceMod
{
    public int amount = 0;
    public int maxAmount = 0;
    public string changeType = ChangeType.ADD;
    /// <summary>
    /// Higher priority is applied last, usually changeType.SET should be 10
    /// </summary>
    public float priority = 0;
    public string modTitle = "";
    /// <summary>
    /// Use to identify and delete ResourceMod once an effect is over
    /// </summary>
    public string resourceModId = "";

    /// <summary>
    /// Use this for buffing, setting resources, etc
    /// </summary>
    public ResourceMod(int amount, int maxAmount, string changeType, string modTitle = "", string resourceModId = "")
    {
        this.amount = amount;
        this.maxAmount = maxAmount;
        this.changeType = changeType;
        this.modTitle = modTitle;
        this.resourceModId = resourceModId;
    }

    /// <summary>
    /// Use for increasing or decreasing max amounts
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="maxAmount"></param>
    public ResourceMod(int amount, int maxAmount)
    {
        this.amount = amount;
        this.maxAmount = maxAmount;
        changeType = ChangeType.ADD;

    }

    /// <summary>
    /// Use this for Damage / Heal or spending mana
    /// </summary>
    /// <param name="amount">Negative for damage</param>
    public ResourceMod(int amount)
    {
        this.amount = amount;
        maxAmount = 0;
        changeType = ChangeType.ADD;
    }
}
