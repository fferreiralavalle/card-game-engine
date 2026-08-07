function Init()
end

function Execute()
end


function HandleOnSelect(ev)
	Node.SetOutputValue("players", { Source.controllerId })
    HandleFinish();
end