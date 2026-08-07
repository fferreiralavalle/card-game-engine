function Init()
	local entitiesAmount = #Inputs.entities
	Node.SetOutputValue("amount", entitiesAmount)
end

function Execute()
	HandleFinish()
end
