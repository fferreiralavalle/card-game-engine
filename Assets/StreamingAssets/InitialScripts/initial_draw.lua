function Init()
	local players = game:GetPlayers()
	for i, player in ipairs(players) do
		local initialHandSize = game.rules.initialHandSize
		local drawEvent = DrawEvent.__new(initialHandSize, player.playerId, player.playerId)
		game:AddEvent(drawEvent)
	end
end
