function Init()

end

function Execute()
	local damage = Damage.__new(tonumber(Inputs.amount))
	Node.SetOutputValue("out", damage)
	HandleFinish()
end
