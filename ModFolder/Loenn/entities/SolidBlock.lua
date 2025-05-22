local defaultFields = require("mods").requireFromPlugin("consts.default_fields")
local core = require("mods").requireFromPlugin("utils.core")
local fakeTilesHelper = require("helpers.fake_tiles")

local fieldTable = {
    width =
    {
        data = defaultFields.width.data,
        info = defaultFields.width.info
    },
    height =
    {
        data = defaultFields.height.data,
        info = defaultFields.height.info
    },
    tileType =
    {
        data = fakeTilesHelper.getPlacementMaterial(),
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options = function()
                return fakeTilesHelper.getTilesOptions(layer)
            end,
            editable = false
        }
    },
    lightOcclude =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    --[[

    crumbleFlag =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    crumbleFlagDelay =
    {
        data = 0,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    crumbleOnTopDelay =
    {
        data = 1,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    crumbleClimbDelay =
    {
        data = 0.6,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    crumbleJumpDelay =
    {
        data = 0.2,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    crumbleSound =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    crumbleUseFlag =
    {
        data = false,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    },
    crumbleUseOnTop =
    {
        data = false,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    },
    crumbleUseClimb =
    {
        data = false,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    },
    crumbleUseJump =
    {
        data = false,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    },
    crumblePermanent =
    {
        data = false,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    }

    ]]
    
}

local crumbleBlock = {
    name = "ChroniaHelper/CrumbleBlock",
    placements =
    {
        name = "CrumbleBlock",
        data =
        {
            blendIn = true
        }
    },
    fieldInformation =
    {
        blendIn =
        {
            fieldType = "boolean",
            allowEmpty = false
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
        "crumbleFlag",
        "crumbleFlagDelay",
        "crumbleOnTopDelay",
        "crumbleClimbDelay",
        "crumbleJumpDelay",
        "crumbleSound",
        "crumbleUseFlag",
        "crumbleUseOnTop",
        "crumbleUseClimb",
        "crumbleUseJump",
        "crumblePermanent",
        "blendIn"
    },
    sprite = fakeTilesHelper.getEntitySpriteFunction("tileType","blendIn")
}

local cornerBoostBlock = {
    name = "ChroniaHelper/CornerBoostBlock",
    placements =
    {
        name = "CornerBoostBlock",
        data =
        {
            blendIn = true
        }
    },
    fieldInformation =
    {
        blendIn =
        {
            fieldType = "boolean",
            allowEmpty = false
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
        "crumbleFlag",
        "crumbleFlagDelay",
        "crumbleOnTopDelay",
        "crumbleClimbDelay",
        "crumbleJumpDelay",
        "crumbleSound",
        "crumbleUseFlag",
        "crumbleUseOnTop",
        "crumbleUseClimb",
        "crumbleUseJump",
        "crumblePermanent",
        "blendIn"
    },
    sprite = fakeTilesHelper.getEntitySpriteFunction("tileType","blendIn")
}

local moonBoostBlock = {
    name = "ChroniaHelper/MoonBoostBlock",
    placements =
    {
        name = "MoonBoostBlock",
        data =
        {
            dashEase = "QuadIn",
            dashMomentum = 8,
            sinkingEase = "SineInOut",
            sinkingMomentum = 1,
            waveRange = 4,
            waveFrequency = 1,
            spawnOffset = false,
            upSpringMomentum = false
        }
    },
    fieldInformation =
    {
        dashEase = defaultFields.ease.info,
        dashMomentum =
        {
            fieldType = "float",
            allowEmpty = false
        },
        sinkingEase = defaultFields.ease.info,
        sinkingMomentum =
        {
            fieldType = "float",
            allowEmpty = false
        },
        waveRange =
        {
            fieldType = "float",
            allowEmpty = false
        },
        waveFrequency =
        {
            fieldType = "float",
            allowEmpty = false
        },
        spawnOffset =
        {
            fieldType = "boolean",
            allowEmpty = false
        },
        upSpringMomentum =
        {
            fieldType = "boolean",
            allowEmpty = false
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
        "dashEase",
        "dashMomentum",
        "sinkingEase",
        "sinkingMomentum",
        "waveRange",
        "waveFrequency",
        "crumbleFlag",
        "crumbleFlagDelay",
        "crumbleOnTopDelay",
        "crumbleClimbDelay",
        "crumbleJumpDelay",
        "crumbleSound",
        "crumbleUseFlag",
        "crumbleUseOnTop",
        "crumbleUseClimb",
        "crumbleUseJump",
        "crumblePermanent",
        "spawnOffset",
        "upSpringMomentum"
    },
    sprite = fakeTilesHelper.getEntitySpriteFunction("tileType","blendIn")
}

local noCoyoteTimeBlock = {
    name = "ChroniaHelper/NoCoyoteTimeBlock",
    placements =
    {
        name = "NoCoyoteTimeBlock",
        data =
        {
            blendIn = true
        }
    },
    fieldInformation =
    {
        blendIn =
        {
            fieldType = "boolean",
            allowEmpty = false
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
        "crumbleFlag",
        "crumbleFlagDelay",
        "crumbleOnTopDelay",
        "crumbleClimbDelay",
        "crumbleJumpDelay",
        "crumbleSound",
        "crumbleUseFlag",
        "crumbleUseOnTop",
        "crumbleUseClimb",
        "crumbleUseJump",
        "crumblePermanent",
        "blendIn"
    },
    sprite = fakeTilesHelper.getEntitySpriteFunction("tileType","blendIn")
}

local solidBlocks = {
    --crumbleBlock,
    cornerBoostBlock,
    moonBoostBlock,
    noCoyoteTimeBlock
}

core.fieldCopy(fieldTable, solidBlocks)

return solidBlocks