function Init()
end

function Execute()
    local targetEntities = {}

    if Inputs ~= nil and Inputs.zones ~= nil then
        -- Native Lua table -> ipairs works!
        for i, zoneCategory in ipairs(Inputs.zones) do
            
            local opponents = Game:GetOpponents(Source.controllerId)
            
            if opponents ~= nil then
                -- C# List<Player> -> 1-based index loop with .Count
                for j = 0, opponents.Count - 1 do
                    local opponent = opponents[j]
                    local zoneObj = Game:GetZoneFromPlayer(zoneCategory, opponent.playerId)
                    
                    if zoneObj ~= nil then
                        local entities = zoneObj:GetEntities()
                        if entities ~= nil then
                            -- C# List<Entity> -> 1-based index loop with .Count
                            for k = 0, entities.Count - 1 do
                                targetEntities[#targetEntities + 1] = entities[k]
                            end
                        end
                    end
                end
            end
        end
    end

	local targetEvent = TargetEvent.__new(targetEntities, Source)
	targetEvent:SubscribeToDone(HandleOnSelect)
	Game:AddEvent(targetEvent)
end


function HandleOnSelect(ev)
    HandleOutputs(ev)
    HandleFinish();
end