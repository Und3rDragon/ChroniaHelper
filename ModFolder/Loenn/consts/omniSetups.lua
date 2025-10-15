local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local connectedEntities = require("helpers.connected_entities")
local ChroniaHelper = require("mods").requireFromPlugin("helpers.chroniaHelper")
local fo = require("mods").requireFromPlugin("helpers.field_options")

local omniSetups = {}

--[[
local omniSetups = require("mods").requireFromPlugin("consts.omniSetups")
]]

omniSetups.fieldOrder = {
	"x","y",
    "width","height",
    "theme","customSkin",
    "easing","startDelay",
    "delays","nodeSpeeds",
    "ticks","tickDelay",
    "ropeColor","ropeLightColor",
    "backgroundColor","startSound",
    "impactSound","tickSound",
    "returnSound","finishSound",
    "returnDelays", "returnSpeeds",
    "returnEasing", "returnedIrrespondingTime",
    "syncTag", "nodeFlags",
    "touchSensitive",
    -- specific string settings
    "bumperSprite",
    "starColors","starPath",
    "topSurface", "bottomSurface",
    "renderTank", "onDash",
    "indicator", "indicatorExpansion",
    -- bools
    "permanent","waiting","ticking","synced", 
    "dashable", "dashableOnce", "onlyDashActivate","dashableRefill",
    "hideCog", "hideRope","nodeSound","sideFlag",
    "timeUnits",
    -- more options
    "customSound","tweakShakes","tweakReturnParams","displayParameters"
}

--[[
omniSetups.placements = {
    width = 16,
    height = 16,
    delays = "0.2,0.2",
    permanent = false,
    waiting = false,
    ticking = false,
    synced = false,
    customSkin = "",
    backgroundColor = "000000",
    ropeColor = "",
    --default 663931
    ropeLightColor = "","
    --default 9b6157
    easing = "SineIn",
    nodeSpeeds = "1,1",
    ticks = 5,
    tickDelay = 1,
    startDelay = 0.1,
    customSound = false,
    startSound = "event:/CommunalHelperEvents/game/zipMover/normal/start",
    impactSound = "event:/CommunalHelperEvents/game/zipMover/normal/impact",
    tickSound = "event:/CommunalHelperEvents/game/zipMover/normal/tick",
    returnSound = "event:/CommunalHelperEvents/game/zipMover/normal/return",
    finishSound = "event:/CommunalHelperEvents/game/zipMover/normal/finish",
    nodeSound = true;
    startShaking = true,
    nodeShaking = true,
    returnShaking = true,
    tickingShaking = true,
    permanentArrivalShaking = true,
    tweakShakes = false,
    syncTag = "",
    hideRope = false,
    hideCog = false,
    -- dashable params
    dashable = false,
    onlyDashActivate = false,
    dashableOnce = true,
    dashableRefill = false,
    -- return params
    tweakReturnParams = false,
    returnSpeeds = "1,1",
    returnDelays = "0.2,0.2",
    returnedIrrespondingTime = 0.5,
    returnEasing = "SineIn",
    -- time
    timeUnits = false,
    sideFlag = false,
    nodeFlags = "",
}
]]

omniSetups.fieldInfo = {
    backgroundColor = {
        fieldType = "color",
        allowEmpty = true,
    },
    ropeColor = {
        fieldType = "color",
        allowEmpty = true,
    },
    ropeLightColor = {
        fieldType = "color",
        allowEmpty = true,
    },
    easing = {
        fieldType = "list",
        elementOptions = {
            options = ChroniaHelper.easers,
            editable = false,
        },
        allowEmpty = false,
    },
    returnEasing = {
        fieldType = "list",
        elementOptions = {
            options = ChroniaHelper.easers,
            editable = false,
        },
        allowEmpty = false,
    },
    ticks = {
        fieldType = "integer",
    },
    startSound = {
        options = {"event:/CommunalHelperEvents/game/zipMover/normal/start"},
        editable = true,
    },
    impactSound = {
        options = {"event:/CommunalHelperEvents/game/zipMover/normal/impact"},
        editable = true,
    },
    tickSound = {
        options = {"event:/CommunalHelperEvents/game/zipMover/normal/tick"},
        editable = true,
    },
    returnSound = {
        options = {"event:/CommunalHelperEvents/game/zipMover/normal/return"},
        editable = true,
    },
    finishSound = {
        options = {"event:/CommunalHelperEvents/game/zipMover/normal/finish"},
        editable = true,
    },
    customSkin = require("mods").requireFromPlugin("helpers.vivUtilsMig").getDirectoryPathFromFile(true),
    delays = {
        fieldType = "list",
    },
    nodeSpeeds = {
        fieldType = "list",
    },
    returnSpeeds = {
        fieldType = "list",
    },
    returnDelays = {
        fieldType = "list",
    },
    touchSensitive = {
        options = {
            "none", "bottom", "sideways", "always"
        },
        editable = false,
    },
    onDash = {
        options = {
            "Normal", "Bounce", "Rebound",
        },
        editable = false,
    },
    indicator = {
        fieldType = "color",
        allowEmpty = true,
    },
}

return omniSetups