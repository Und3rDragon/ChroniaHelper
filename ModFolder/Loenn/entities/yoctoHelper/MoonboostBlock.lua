local baseEntity = require("mods").requireFromPlugin("helpers.base_entity")
local fieldOptions = require("mods").requireFromPlugin("helpers.field_options")
local fakeTilesHelper = require("helpers.fake_tiles")

local entity = {
    name = "ChroniaHelper/MoonBoostBlock",
    placements =
    {
        name = "MoonBoostBlock",
        data =
        {
            tileType = fakeTilesHelper.getPlacementMaterial(),
            lightOcclude = 1,
            dashAnimEase = "QuadIn",
            dashMomentum = 1,
            sinkAnimEase = "SineInOut",
            sinkMomentum = 1,
            waveRange = 4,
            waveFrequency = 1,
            triggerSound = "",
            collapseSound = "",
            crumbleDelayOnClimb = - 1,
            crumbleDelayTouchTop = - 1,
            crumbleDelayTouchBottom = - 1,
            crumbleDelayTouchLeft = - 1,
            crumbleDelayTouchRight = - 1,
            crumbleDelayJumpoff = - 1,
            crumblePermanent = false,
            blendIn = false,
            spawnOffset = false,
            upSpringReactionForce = false
        }
    },
    fieldInformation =
    {
        tileType =
        {
            options = function()
                return fakeTilesHelper.getTilesOptions()
            end,
            editable = false
        },
        lightOcclude =
        {
            minimumValue = 0,
            maximumValue = 1
        },
        dashAnimEase = fieldOptions.easeMode,
        sinkAnimEase = fieldOptions.easeMode,
        crumbleDelayOnClimb =
        {
            minimumValue = - 1
        },
        crumbleDelayTouchTop =
        {
            minimumValue = - 1
        },
        crumbleDelayTouchBottom =
        {
            minimumValue = - 1
        },
        crumbleDelayTouchLeft =
        {
            minimumValue = - 1
        },
        crumbleDelayTouchRight =
        {
            minimumValue = - 1
        },
        crumbleDelayJumpoff =
        {
            minimumValue = - 1
        }
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "tileType",
        "lightOcclude",
        "dashAnimEase",
        "dashMomentum",
        "sinkAnimEase",
        "sinkMomentum",
        "waveRange",
        "waveFrequency",
        "triggerSound",
        "collapseSound",
        "crumbleDelayOnClimb",
        "crumbleDelayTouchTop",
        "crumbleDelayTouchBottom",
        "crumbleDelayTouchLeft",
        "crumbleDelayTouchRight",
        "crumbleDelayJumpoff",
        "crumblePermanent",
        "blendIn",
        "spawnOffset",
        "upSpringReactionForce"
    },
    sprite = fakeTilesHelper.getEntitySpriteFunction("tileType","blendIn")
}

baseEntity.invoke(entity)

return entity