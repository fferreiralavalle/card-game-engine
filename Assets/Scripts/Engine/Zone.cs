using MoonSharp.Interpreter;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A place where multiple entities exist. Each zone can have it's own rules. Ex Deck, Hand, Graveyard, Battlefield, etc.
/// </summary>
[Serializable]
[MoonSharpUserData]
public class Zone
{
    /// <summary>
    /// Deck, Hand, Graveyard, Exile, Battlefield, etc
    /// </summary>
    public string zoneCategory = "";
    /// <summary>
    /// Entities inside this zone
    /// </summary>
    public List<Entity> entities = new List<Entity> ();
    /// <summary>
    /// The max amount of entities that can exist at the same time in the zone
    /// </summary>
    public int maxEntityAmountPerPlayer = 9999;
    /// <summary>
    /// Player who owns the zone, null equals no owner
    /// </summary>
    public string ownerId = null;
    /// <summary>
    /// Default permissions for when a card enters it
    /// Ex: isPublic, allowPlayFrom, allowAttackFrom, isHiddenToOpponent
    /// </summary>
    public Dictionary<string, bool> zonePermissions = new Dictionary<string, bool> ();


    public List<Entity> GetEntities()
    {
        return entities;
    }
}
