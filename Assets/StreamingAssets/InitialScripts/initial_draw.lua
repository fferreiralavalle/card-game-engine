function Init()
    local players = Game:GetPlayers()
    for i = 0, players.Count - 1 do
		local player = players[i]
		local initialHandSize = Game.rules.initialHandSize
		local drawEvent = DrawEvent.__new(initialHandSize, player.playerId, player.playerId)
		Game:AddEvent(drawEvent)
	end
end