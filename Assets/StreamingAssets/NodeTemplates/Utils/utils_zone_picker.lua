function Init()
	
end

function Execute()
	Debug.Log("DEBUG: Executing utils_zone_picker.lua")
    Node.SetOutputValue("zones", Fields.zones)
	HandleFinish()
end
