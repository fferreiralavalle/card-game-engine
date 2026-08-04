using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;

[Serializable]
[MoonSharpUserData]
public class Damage
{
    /// <summary>
    /// How much damage you want to deal
    /// </summary>
    public int amount = 1;
    /// <summary>
    /// Types could be: "fire", "cold", "dark", "holy", etc.
    /// </summary>
    public List<string> typeIds = new List<string>();
    /// <summary>
    /// Use tags to reference specific damages, such from "claw" cards. You can then buff all "claw" cards
    /// </summary>
    public List<string> tags = new List<string>();

    public Damage(int amount)
    {
        this.amount = amount;
    }

    public Damage(int amount, List<string> tags)
    {
        this.amount = amount;
        this.tags = tags;
    }
}
