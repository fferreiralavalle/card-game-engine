function Init()
	Node:SetOutputValue("targets", { Source })
end

function Execute()
	Init()
	HandleFinish();
end
