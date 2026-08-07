function Init()

end

function Execute()
	local amount = tonumber(Inputs.amount) or 1
	local playerId = Inputs.playerIds[1]
	Debug.Log(Inputs.playerIds)
	Debug.Log(playerId)
	local drawEvent = DrawEvent.__new(amount, playerId, playerId)

	HandleEventSetup(drawEvent)

	game:AddEvent(drawEvent)
	HandleFinish()
end
