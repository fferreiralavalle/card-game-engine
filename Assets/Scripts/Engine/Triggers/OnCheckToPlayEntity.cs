using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic trigger to set the Playability rules of your card game
/// </summary>
public class OnCheckToPlayEntity : Trigger
{
    public OnCheckToPlayEntity(): base(new List<string> (){ EventUtils.Try("check_to_play")})
    {
        maxTriggers = int.MaxValue;
    }


}
