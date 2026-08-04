function Init()

end

function Execute()
	for i = 1, Inputs.amount do
    	HandleFlow("do")
	end
	HandleFinish()
end
