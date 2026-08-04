function Init()

end

function Execute()
	for index, value in ipairs(Inputs.entities) do
    	HandleFlow("do")
	end
	HandleFinish()
end
