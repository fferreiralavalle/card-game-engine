using System.Threading.Tasks;

public class EndTurnEvent : Event
{
    public string nextPlayerId;

    public EndTurnEvent(string nextPlayerId)
    {
        this.nextPlayerId = nextPlayerId;
        eventType = "end_turn";
    }

    public EndTurnEvent(){}

    protected override Task Execute(Game game)
    {
        if (string.IsNullOrEmpty(nextPlayerId))
        {
            nextPlayerId = game.GetNextPlayerTurn();
        }
        game.AddEvent(new StartTurnEvent(nextPlayerId));
        return base.Execute(game);
    }
}
