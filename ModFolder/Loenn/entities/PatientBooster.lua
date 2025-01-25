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
                refillDashes = "",
                refillStamina = true,
                customHitbox = "c,10,0,2",
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
        return entity.red and "objects/ChroniaHelper/customBoosterPresets/red/loop0" or "objects/ChroniaHelper/customBoosterPresets/green/loop0"
    end,
    selection = function (room, entity)
        return utils.rectangle(entity.x - 11, entity.y - 9, 22, 18)
    end,
}

return patientBooster
