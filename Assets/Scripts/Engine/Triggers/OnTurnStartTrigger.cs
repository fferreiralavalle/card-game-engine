using System.Collections.Generic;

public class OnTurnStartTrigger : Trigger
{
    public List<string> playerIds = new List<string>();

    public OnTurnStartTrigger(List<string> playerIds): base(new List<string> { EventUtils.Done(StartTurnEvent.GetEventType()) })
    {
        this.playerIds = playerIds;
    }

    public override bool ShouldTrigger(Event eve, Game game)
    {
        if (eve is StartTurnEvent ste)
        {
            if (playerIds.Count > 0 && playerIds.Contains(ste.playerId))
            {
                return base.ShouldTrigger(eve, game);
            }
        }
        return false;
    }
}
