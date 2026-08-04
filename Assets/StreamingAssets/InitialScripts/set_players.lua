function Init()
    local players = Game:GetPlayers()
    for i = 0, players.Count - 1 do
		local player = players[i]
		Game.CreateEntityEvent("Hero", "PLAYER", player.playerId)
	end
end