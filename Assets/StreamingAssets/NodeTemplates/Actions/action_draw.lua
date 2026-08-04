function Init()

end

function Execute()
	local amount = tonumber(Inputs.amount) or 1
    local playerId = tonumber(Inputs.playerId) or 0
	local drawEvent = DrawEvent.__new(amount, playerId, playerId)

	HandleEventSetup(drawEvent)

	Game:AddEvent(drawEvent)
	HandleFinish()
end
