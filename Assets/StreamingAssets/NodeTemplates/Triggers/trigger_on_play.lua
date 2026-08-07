function Init()
	-- In Lua, simple tables/lists use curly braces {}
	local entityId = Source.runtimeId
	local eventTypeDone = { "play_entity.done" }
	local eventTypeTry = { "play_entity.try" }

	-- 1. In Lua, instantiate C# objects
	local triggerDone = OnPlayEntityTrigger.__new(entityId, eventTypeDone)
	local triggerTry = OnPlayEntityTrigger.__new(entityId, eventTypeTry)

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
