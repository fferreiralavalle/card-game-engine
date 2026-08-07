function Init()

end

function Execute()
	local damageEvent = DealDamageEvent.__new(Inputs.damage, Inputs.targets)
	damageEvent.entitySource = Source;
	HandleEventSetup(damageEvent)

	game:AddEvent(damageEvent)
	HandleFinish()
end
