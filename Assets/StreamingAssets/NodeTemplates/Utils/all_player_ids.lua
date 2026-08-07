function Init()
	local playerIds = {}
	local players = game:GetPlayers();
	-- C# List<Player> -> 1-based index loop with .Count
	for i, player in ipairs(players) do
		playerIds[#playerIds + 1] = player.playerId
	end

	Node.SetOutputValue("players", playerIds)
end

function Execute()
	Init()
	HandleFinish()
end
