function Init()
	local eventIds = { "start_turn.done" }
	local onTurnStartTrigger = Trigger.__new(eventIds)
	onTurnStartTrigger:Subscribe(HandleTurnEnd)
	game:AddTrigger(onTurnStartTrigger)
end

function HandleTurnEnd(eve, trigger)
	local playerId = eve.output.playerId
	game:ModifyPlayerResource(playerId, "mana", ResourceMod.__new(999))
end