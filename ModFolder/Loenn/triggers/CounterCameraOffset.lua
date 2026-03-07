local trigger = {}

trigger.name = "ChroniaHelper/CounterCameraOffset"

trigger.placements = {
	name = "offset",
	data = {
		width = 8,    
        height = 8,
		counter = "cameraIndex",
		param1 = "0",
		param2 = "0",
		requirement = 0,
		offsetXFrom = "",
		offsetXTo = "0",
		offsetYFrom = "",
		offsetYTo = "0",
		positionMode = 0,
		cameraMode = 0,
		leaveReset = false,
		onlyOnce = false,
	},
}

trigger.fieldInformation = {
	requirement = {
		fieldType = "integer",
		options = {
			["Disabled"] = 0,
			["Match Param1"] = 1,
			["Not Match Param1"] = 2,
			["Between Param1 and Param2"] = 3,
			["Not Between Param1 and Param2"] = 4,
		},
		editable = false,
	},
	positionMode = {
		fieldType = "integer",
        options = {
            ["NoEffect"] = 0,
            ["RightToLeft"] = 1,
            ["LeftToRight"] = 2,
            ["BottomToTop"] = 3,
            ["TopToBottom"] = 4,
            ["VerticalCenter"] = 5,
            ["HorizontalCenter"] = 6,
        },
        editable = false,
    },
	cameraMode = {
		fieldType = "integer",
		options = {
			["Normal"] = 0,
			["X Only"] = 1,
			["Y Only"] = 2,
		},
		editable = false,
	},
}

return trigger