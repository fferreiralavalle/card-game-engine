function Init()
	local eventIds = { "start_turn.done" }
	local onTurnStartTrigger = Trigger.__new(eventIds)
	onTurnStartTrigger:Subscribe(HandleTurnEnd)
	Game:AddTrigger(onTurnStartTrigger)
end

function HandleTurnEnd(eve, trigger)
	local playerId = eve.output.playerId
	Game:ModifyPlayerResource(playerId, "mana", ResourceMod.__new(0,1))
	Game:ModifyPlayerResource(playerId, "mana", ResourceMod.__new(999))
end