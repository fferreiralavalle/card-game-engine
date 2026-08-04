using RuntimeCardEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Example rule that verifies the card's play costs are satisfiable by the controller's resources.
/// Uses reflection to attempt to query resource amounts from the Game object.
/// If resource amounts cannot be discovered it will not block (fail-open) to avoid accidentally locking modded games.
/// </summary>
public class PlayCostRule : IPlayabilityRule
{
    public PlayabilityResult Check(Game game, Entity card)
    {
        if (game == null) throw new ArgumentNullException(nameof(game));
        if (card == null) throw new ArgumentNullException(nameof(card));

        try
        {
            var playCosts = card.GetPlayCosts();
            if (playCosts == null) return PlayabilityResult.Success();

            // If card has zero costs defined, allow
            bool anyCostDefined = false;
            var failReasons = new List<string>();

            foreach (PlayCost pc in playCosts)
            {
                if (pc == null || pc.costs == null || pc.costs.Count == 0)
                {
                    // A free cost option exists => playable
                    return PlayabilityResult.Success();
                }

                anyCostDefined = true;

                bool thisCostPayable = true;
                var localReasons = new List<string>();

                foreach (var kvp in pc.costs)
                {
                    string resourceId = kvp.Key;
                    object expectedResourceObj = kvp.Value;
                    // Try to extract required amount from the PlayCost's Resource using common property names
                    int requiredAmount = kvp.Value.GetAmount();
                    // Get the player's available amount for this resourceId from Game 
                    int playerResourceAmount = game.GetPlayerResourceAmount(card.controllerId, resourceId);
                    // Query the player's available amount for resourceId
                    if (playerResourceAmount < requiredAmount)
                    {
                        // Could not find resource amounts on the Game; fail-open (allow play)
                        thisCostPayable = false;
                        localReasons.Add($"Need {requiredAmount} {resourceId} (have {playerResourceAmount})");
                    }
                }

                if (thisCostPayable)
                {
                    // At least one cost option is payable => success
                    return PlayabilityResult.Success();
                }
                // otherwise collect reasons per-cost option (we'll show reasons for all options)
                failReasons.Add($"Cost option '{pc.playCostName ?? pc.playCostId ?? "<unnamed>"}' not payable: {string.Join("; ", pc.costs.Keys)}");
            }

            if (!anyCostDefined)
                return PlayabilityResult.Success();

            // None of cost options are payable
            if (failReasons.Count == 0)
                return PlayabilityResult.Fail("No payable cost options");
            return PlayabilityResult.Fail(failReasons);
        }
        catch (Exception ex)
        {
            // Do not block play on unexpected errors; expose reason for debugging
            return PlayabilityResult.Fail($"PlayCostRule error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    
}
