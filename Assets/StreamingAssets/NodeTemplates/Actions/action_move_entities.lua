function Init()

end

function Execute()
	local moveEvent = MoveToZoneEvent.__new(Inputs.targets, Inputs.zoneId, Inputs.ownerId)
	moveEvent.entitySource = Source;
	HandleEventSetup(moveEvent)

	Game:AddEvent(moveEvent)
	HandleFinish()
end
