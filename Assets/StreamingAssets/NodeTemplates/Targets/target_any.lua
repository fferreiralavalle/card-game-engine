function Init()
end

function Execute()
	local targetEntities = {}

	if Inputs ~= nil and Inputs.zoneIds ~= nil then
		-- Native Lua table -> ipairs works!
		for i, zoneCategory in ipairs(Inputs.zoneIds) do
			if Inputs.ownerIds ~= nil then
				-- C# List<string> -> 1-based index loop with .Count
				for j, playerId in ipairs(Inputs.ownerIds) do
					local zoneObj = game:GetZoneFromPlayer(zoneCategory, playerId)

					if zoneObj ~= nil then
						local entities = zoneObj:GetEntities()

						if entities ~= nil then
							-- C# List<Entity> -> 1-based index loop with .Count
							for k, entity in ipairs(entities) do
								targetEntities[#targetEntities + 1] = entity
							end
						end
					end
				end
			end
		end
	end

	local targetEvent = TargetEvent.__new(targetEntities, Source)
	targetEvent:SubscribeToDone(HandleOnSelect)
	game:AddEvent(targetEvent)
end

function HandleOnSelect(ev)
	HandleOutputs(ev)
	HandleFinish();
end
