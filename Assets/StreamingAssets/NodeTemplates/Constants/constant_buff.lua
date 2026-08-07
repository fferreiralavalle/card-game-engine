function Init()

end

function Execute()
	-- Safely convert inputs to integer values (compatible with MoonSharp / Lua 5.2)
	local health = Fields.health and math.floor(tonumber(Fields.health)) or 0
	local maxHealth = Fields.maxHealth and math.floor(tonumber(Fields.maxHealth)) or 0
	local attack = Fields.attack and math.floor(tonumber(Fields.attack)) or 0
	local maxAttack = Fields.maxAttack and math.floor(tonumber(Fields.maxAttack)) or 0

	local healthMod = ResourceMod.__new(health, maxHealth, true)
	healthMod.changeType = Fields.changeType
	local healthChange = ResourceChange.__new("health", healthMod)

	local attackMod = ResourceMod.__new(attack, maxAttack, true)
	attackMod.changeType = Fields.changeType
	local attackChange = ResourceChange.__new("attack", attackMod)

	local resourceChanges = List_ResourceChange.__new()
	resourceChanges.Add(healthChange)
	resourceChanges.Add(attackChange)
	Node.SetOutputValue("out", resourceChanges)
	HandleFinish()
end
