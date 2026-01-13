local trigger = {}

trigger.name = "ChroniaHelper/StopclockTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        stopclockName = "stopclock",
        time = "00:01:00:000",
        operation = 0,
        countdown = true,
        onlyOnce = true,
        followLevelPause = false,
    },
}

trigger.fieldInformation = {
    operation = {
        options = {
            ["Set"] = 0,
            ["Add"] = 1,
            ["Minus"] = 2,
            ["Stop"] = 3,
            ["Start/Resume"] = 4,
            ["Set and Start"] = 5,
        },
        editable = false,
    },
}

trigger.triggerText = function(room, entity)
	local base = "Stopclock"
    
    if entity.countdown then
        base = "Countdown " .. base
    end
    
    base = base .. " (tag = " .. entity.stopclockName .. ") (time = " .. entity.time .. ")"
    
	return base
end

return trigger