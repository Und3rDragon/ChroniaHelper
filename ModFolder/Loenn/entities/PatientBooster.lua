local utils = require("utils")

local patientBooster = {
    name = "ChroniaHelper/PatientBooster",
    depth = -8500,
    placements = {
        default = {
            data = {
                red = false,
                sprite = "",
                respawnDelay = 1.0,
                --refillDashes = "",
                --refillStamina = true,
                customHitbox = "c,10,0,2",
                stamina = 110,
                dashes = 1,
                staminaMode = 0,
                dashesMode = 0,
                killIfStayed = -1,
            }
        },
        {
            name = "default",
            data = {
                red = false,
            }
        },
        {
            name = "red",
            data = {
                red = true,
            }
        }
    },
    texture = function (room, entity)
        if entity.killIfStayed > 0 then
            return "objects/ChroniaHelper/customBoosterPresets/yellow/loop0"
        elseif entity.red then
            return "objects/ChroniaHelper/customBoosterPresets/red/loop0"
        else
            return "objects/ChroniaHelper/customBoosterPresets/green/loop0"
        end
    end,
    selection = function (room, entity)
        return utils.rectangle(entity.x - 11, entity.y - 9, 22, 18)
    end,
    fieldInformation = {
        staminaMode = {
            options = {
                ["refill"] = 0,
                ["set"] = 1,
            },
            editable = false,
        },
        dashesMode = {
            options = {
                ["refill"] = 0,
                ["set"] = 1,
            },
            editable = false,
        },
        stamina = {
            fieldType = "integer",
        },
        dashes = {
            fieldType = "integer",
        },
        sprite = {
            options = {
                "Preset_green",
                "Preset_red",
                "Preset_yellow",
                "Default_booster",
                "Preset_pink",
            },
            editable = true,
        },
    },
}

return patientBooster
