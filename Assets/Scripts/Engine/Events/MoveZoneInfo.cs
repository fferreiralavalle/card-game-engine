using MoonSharp.Interpreter;
using System;
using UnityEngine;
[Serializable]
[MoonSharpUserData]
public class MoveZoneInfo
{
    public Entity entity;
    public Zone originalZone;
    public Zone targetZone;

    public MoveZoneInfo(Entity entity, Zone originalZone, Zone targetZone)
    {
        this.entity = entity;
        this.originalZone = originalZone;
        this.targetZone = targetZone;
    }
}
