function Init()
	local eventIds = { "start_turn.done" }
	local onTurnStartTrigger = Trigger.__new(eventIds)
	onTurnStartTrigger:Subscribe(HandleTurnStart)
	game:AddTrigger(onTurnStartTrigger)
end

function HandleTurnStart(eve, trigger)
	local playerId = eve.output.playerId
	local drawEvent = DrawEvent.__new(1, playerId, playerId)
	game:AddEvent(drawEvent)
end