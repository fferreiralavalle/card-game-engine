using System.Threading.Tasks;
using UnityEngine;

public class StartTurnEvent : Event
{
    public string playerId;

    public StartTurnEvent(string playerId)
    {
        this.playerId = playerId;
        eventType = "start_turn";
        SetOutput();
    }

    public StartTurnEvent() { }

    protected override Task Execute(Game game)
    {
        game.SetActivePlayer(playerId);
        SetOutput();
        return base.Execute(game);
    }

    public override void SetOutput()
    {
        base.SetOutput();
        output["playerId"] = playerId;
    }
}
