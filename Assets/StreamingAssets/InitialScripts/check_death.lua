function Init()
    local deathCheckTrigger = Trigger.__new({ "change_properties.done" })
    
    deathCheckTrigger:Subscribe(function(event, trigger)
        local zones = Game:GetAllZonesWithCategory("FIELD")
        
        if zones ~= nil then
            local deadEntities = {}

            -- C# List<Zone> uses 0-based indexing (0 to Count - 1)
            for i = 0, zones.Count - 1 do
                local zone = zones[i]
                if zone ~= nil then
                    local entities = zone:GetEntities()
                    if entities ~= nil then
                        -- C# List<Entity> uses 0-based indexing (0 to Count - 1)
                        for j = 0, entities.Count - 1 do
                            local entity = entities[j]
                            if entity ~= nil and entity:GetPropertyValue("health") <= 0 then
                                deadEntities[#deadEntities + 1] = entity
                            end
                        end
                    end
                end
            end

            -- deadEntities is a native Lua table, so ipairs works smoothly here
            for _, entity in ipairs(deadEntities) do
                local entityList = List_Entity.__new()
                entityList:Add(entity)
                
                local moveEvent = MoveToZoneEvent.__new(entityList, "GRAVE", entity.controllerId)
				-- We want the event to go last for visual reasons
				moveEvent.priority = -10
                Game:AddEvent(moveEvent)
            end
        end
    end)
    
    Game:AddTrigger(deathCheckTrigger)
end