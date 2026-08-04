using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[MoonSharpUserData]
public class GameRules
{
    public string resourceIdForDamage = CommonResources.ATTACK;
    public string resourceIdForHealth = CommonResources.HEALTH;
    public string defaultPlayZone = CommonZones.STACK;
    public string defaultPermanentZone = CommonZones.FIELD;
    public string defaultDiscardZone = CommonZones.GRAVE;
    public string defaultOverflowZone = CommonZones.GRAVE;
    public string defaultPlayerResourceZone = CommonZones.PLAYERS;
    public int initialHandSize = 4;
}
