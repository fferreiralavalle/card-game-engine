function Init()

end

function Execute()
	for clave, valor in pairs(Fields) do
    	Node.SetOutputValue(clave, valor)
	end
	HandleFinish()
end
