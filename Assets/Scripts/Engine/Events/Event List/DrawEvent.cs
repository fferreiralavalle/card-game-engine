using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Put the top card from a players deck into a players hand.
/// </summary>
[MoonSharpUserData]
[Serializable]
public class DrawEvent : Event
{
    public int amount = 1;
    public string deckOwnerId = "";
    public string handOwnerId = "";

    public DrawEvent(int amount, string deckOwnerId, string handOwnerId)
    {
        this.amount = amount;
        this.deckOwnerId = deckOwnerId;
        this.handOwnerId = handOwnerId;
        eventType = "draw";
        eventTags.Add("draw");
    }

    protected override async Task Execute(Game game)
    {
        await base.Execute(game);
        Zone deck = game.GetZoneFromPlayer(CommonZones.DECK, deckOwnerId);

        List<Entity> topCards = deck.entities.Take(amount).ToList();

        MoveToZoneEvent moveZonesEvent = new MoveToZoneEvent(topCards, CommonZones.HAND, handOwnerId);
        moveZonesEvent.eventTags.Add("draw");
        game.AddEvent(moveZonesEvent);
    }
}
