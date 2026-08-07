using MoonSharp.Interpreter;
using System.Threading.Tasks;
using UnityEngine;

[MoonSharpUserData]
public class StartTurnEvent : Event
{
    public string playerId;

    public StartTurnEvent(string playerId)
    {
        this.playerId = playerId;
        eventType = GetEventType();
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

    public static string GetEventType()
    {
        return "start_turn";
    }
}
