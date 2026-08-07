function Init()
	Node.SetOutputValue("zones", Fields.zones)
end

function Execute()
	Node.SetOutputValue("zones", Fields.zones)
	HandleFinish()
end
