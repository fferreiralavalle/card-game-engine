function Init()

end

function Execute()
	local entitiesAmount = #Inputs.entities
	Node.SetOutputValue("amount", entitiesAmount)
	HandleFinish()
end
