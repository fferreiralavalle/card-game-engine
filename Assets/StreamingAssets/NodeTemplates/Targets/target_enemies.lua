function Init()
end

function Execute()
	local targetEntities = {}

	-- Native Lua table -> ipairs works!
	for i, zoneCategory in ipairs(Inputs.zones) do
		local opponents = game:GetOpponents(Source.controllerId)
		-- C# List<Player> -> 1-based index loop with .Count
		for j, opponent in ipairs(opponents) do
			local zoneObj = game:GetZoneFromPlayer(zoneCategory, opponent.playerId)

			local entities = zoneObj:GetEntities()

			-- C# List<Entity> -> 1-based index loop with .Count
			for k, entity in ipairs(entities) do
				targetEntities[#targetEntities + 1] = entity
			end
		end
	end

	Node.SetOutputValue("targets", targetEntities)
	HandleFinish()
end
