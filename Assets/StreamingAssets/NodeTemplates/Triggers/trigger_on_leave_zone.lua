function Init()
	-- In Lua, simple tables/lists use curly braces {}
	local eventTypeDone = { "move_to_zone.done" }
	local eventTypeTry = { "move_to_zone.try" }
	local zoneIds = Inputs.zoneIds

	-- 1. In Lua, instantiate C# objects
	local triggerDone = OnEntityMoveFromZoneTrigger.__new({ Source }, zoneIds)
	local triggerTry = OnEntityMoveFromZoneTrigger.__new({ Source }, zoneIds)
	triggerDone.eventTypes = eventTypeDone
	triggerTry.eventTypes = eventTypeTry

	-- 2. MoonSharp handles C# event subscriptions via :add() instead of +=
	triggerDone:Subscribe(HandleOnPlayDone)
	triggerTry:Subscribe(HandleOnPlayTry)

	-- Pass registered triggers back to C#
	game:AddTrigger(triggerDone)
	game:AddTrigger(triggerTry)
end

function HandleOnPlayDone(ev, trigger)
	HandleOutputs(ev)
	HandleFlow("onDone")
end

function HandleOnPlayTry(ev, trigger)
	HandleOutputs(ev)
	HandleFlow("onTry")
end

function Execute()
	HandleFinish()
end
