using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Entities can have multiple ways of paying for them.
/// Use Tags for things such as "kicker" to add additional effects when played
/// </summary>
[Serializable]
[MoonSharpUserData]
public class PlayCost
{
    /// <summary>
    /// Use to identify and later remove for aura effects that add optional costs
    /// </summary>
    public string playCostId = "";
    /// <summary>
    ///  Displayed when selecting between multiple costs
    /// </summary>
    public string playCostName = "";
    /// <summary>
    /// Cost to play the entity: Mana, health, armor, etc.
    /// </summary>
    public Dictionary<string, Resource> costs = new Dictionary<string, Resource>();
    /// <summary>
    /// Use Tags for things such as "kicker" to add additional effects when played
    /// </summary>
    public List<string> tags = new List<string>();
    /// <summary>
    /// May include things such as grave, Exile, Extra, Etc. Hand should be default in most games
    /// </summary>
    public List<string> playableZones = new List<string>();
}
