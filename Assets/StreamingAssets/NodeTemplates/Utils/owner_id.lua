function Init()

end

function Execute()
	Node.SetOutputValue("targets", Source.controllerId)
	HandleFinish()
end
