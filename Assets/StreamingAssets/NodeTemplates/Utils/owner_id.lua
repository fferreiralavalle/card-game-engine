function Init()
	Node.SetOutputValue("targets", { Source.controllerId })
end

function Execute()
	Node.SetOutputValue("targets", { Source.controllerId })
	HandleFinish()
end
