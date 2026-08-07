function Init()
	local players = game:GetPlayers()
	for i, player in ipairs(players) do
		game:CreateEntityEvent("Hero", "PLAYER", player.playerId)
	end
end

function GetPriority()
	return 100
end
