using System.Threading.Tasks;

public class EndTurnEvent : Event
{
    public string nextPlayerId;

    public EndTurnEvent(string nextPlayerId)
    {
        this.nextPlayerId = nextPlayerId;
    }

    public EndTurnEvent(){}

    protected override Task Execute(Game game)
    {
        if (string.IsNullOrEmpty(nextPlayerId))
        {
            nextPlayerId = game.GetNextPlayerTurn();
        }
        game.SetActivePlayer(nextPlayerId);
        return base.Execute(game);
    }
}
