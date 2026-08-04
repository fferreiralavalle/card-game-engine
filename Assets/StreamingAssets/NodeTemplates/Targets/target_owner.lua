function Init()
end

function Execute()
	local targetEvent = TargetEvent.__new(targetEntities, Source.controllerId)
	targetEvent:SubscribeToDone(HandleOnSelect)
	Game:AddEvent(targetEvent)
end


function HandleOnSelect(ev)
	Node.SetOutputValue("players", { Source.controllerId })
    HandleFinish();
end