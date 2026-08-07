function Init()

end

function Execute()
	local changeEvent = ChangeResourceEvent.__new(Inputs.targets, Inputs.resources)
	changeEvent.entitySource = Source;
	HandleEventSetup(changeEvent)

	game:AddEvent(changeEvent)
	Node.SetOutputValue("out", Inputs.resources)
	HandleFinish()
end

function Remove()
	local changeList = Inputs.resources
	local changeIds = {}
	for j = 0, changeList.Count - 1 do
		local change = changeList[j]
		changeIds[#changeIds + 1] = change.resourceMod.resourceModId
	end

	local removeEvent = RemoveAttributeChangeEvent:__new(Inputs.targets, changeIds)
	game:AddEvent(removeEvent)
	HandleFinish()
end
