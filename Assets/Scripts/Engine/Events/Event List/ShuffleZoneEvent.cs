using System.Threading.Tasks;

public class ShuffleZoneEvent : Event
{
    public string zoneCategory = "";
    public string ownerId = "";

    public ShuffleZoneEvent(string zoneCategory, string ownerId)
    {
        this.zoneCategory = zoneCategory;
        this.ownerId = ownerId;
    }

    protected override async Task Execute(Game game)
    {
        await base.Execute(game);

        Zone zone = game.GetZoneFromPlayer(zoneCategory, ownerId);
        if (zone == null)
        {
            return;
        }
        GameUtils.Shuffle(new System.Random(), zone.entities.ToArray());
    }
}
