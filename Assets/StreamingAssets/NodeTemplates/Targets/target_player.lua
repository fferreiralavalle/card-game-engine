function Init()
end

function Execute()
	local targetEntities = {}

	local players = game:GetPlayers()

	-- C# List<Player> -> 1-based index loop with .Count
	for j, player in ipairs(players) do
		local zoneObj = game:GetZoneFromPlayer("PLAYER", player.playerId)

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

	local targetEvent = TargetEvent.__new(targetEntities, Source)
	targetEvent:SubscribeToDone(HandleOnSelect)
	game:AddEvent(targetEvent)
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
