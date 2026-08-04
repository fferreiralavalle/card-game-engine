function Init()
end

function Execute()
    local targetEntities = {}

	local opponents = Game:GetPlayers()
	
	if opponents ~= nil then
		-- C# List<Player> -> 1-based index loop with .Count
		for j = 0, opponents.Count - 1 do
			local opponent = opponents[j]
			local zoneObj = Game:GetZoneFromPlayer("PLAYER", opponent.playerId)
			
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

	local targetEvent = TargetEvent.__new(targetEntities, Source)
	targetEvent:SubscribeToDone(HandleOnSelect)
	Game:AddEvent(targetEvent)
end


function HandleOnSelect(ev)
	local players = {}
	local heroes = ev.output["targets"];
	for i = 0, heroes.Count - 1 do
		players[#players + 1] = heroes[i].controllerId;
	end
	Node.SetOutputValue("players", players)
    HandleFinish();
end