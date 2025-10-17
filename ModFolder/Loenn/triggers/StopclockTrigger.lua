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
    },
}

trigger.fieldInformation = {
    operation = {
        options = {
            ["Set"] = 0,
            ["Add"] = 1,
            ["Minus"] = 2,
            ["Stop"] = 3,
            ["Resume"] = 4,
        },
        editable = false,
    },
}

return trigger