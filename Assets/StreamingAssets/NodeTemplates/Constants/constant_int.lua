function Init()
	for clave, valor in pairs(Fields) do
		Node.SetOutputValue(clave, valor)
	end
end

function Execute()
	Init()
	HandleFinish()
end
