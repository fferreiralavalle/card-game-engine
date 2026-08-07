function Init()
	local deathCheckTrigger = Trigger.__new({ "change_properties.done" })

	deathCheckTrigger:Subscribe(function(event, trigger)
		local zones = game:GetAllZonesWithCategory("FIELD")

		if zones ~= nil then
			local deadEntities = {}

			-- C# List<Zone> uses 0-based indexing (0 to Count - 1)
			for i, zone in ipairs(zones) do
				if zone ~= nil then
					local entities = zone:GetEntities()
					if entities ~= nil then
						-- C# List<Entity> uses 0-based indexing (0 to Count - 1)
						for j, entity in ipairs(entities) do
							if entity ~= nil and entity:GetPropertyValue("health") <= 0 then
								deadEntities[#deadEntities + 1] = entity
							end
						end
					end
				end
			end

			-- deadEntities is a native Lua table, so ipairs works smoothly here
			for _, entity in ipairs(deadEntities) do
				local entityList = { entity }

				local moveEvent = MoveToZoneEvent.__new(entityList, "GRAVE", entity.controllerId)
				-- We want the event to go last for visual reasons
				moveEvent.priority = -10
				game:AddEvent(moveEvent)
			end
		end
	end)

	game:AddTrigger(deathCheckTrigger)
end
