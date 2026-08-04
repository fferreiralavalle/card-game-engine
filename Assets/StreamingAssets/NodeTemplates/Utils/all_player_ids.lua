function Init()

end

function Execute()
	local playerIds = {}
	local players = Game:GetPlayers();
	if players ~= nil then
		-- C# List<Player> -> 1-based index loop with .Count
		for j = 0, players.Count - 1 do
			local player = players[j]
			playerIds[#playerIds + 1] = player.playerId
		end
	end

	Node.SetOutputValue("players", playerIds)
	HandleFinish()
end
