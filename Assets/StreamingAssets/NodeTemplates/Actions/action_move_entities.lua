function Init()

end

function Execute()
	local moveEvent = MoveToZoneEvent.__new(Inputs.targets, Inputs.zoneId, Inputs.ownerId)
	moveEvent.entitySource = Source;
	HandleEventSetup(moveEvent)

	game:AddEvent(moveEvent)
	HandleFinish()
end
