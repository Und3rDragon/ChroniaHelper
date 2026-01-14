local utils = require("utils")

local balloon = {}

balloon.name = "ChroniaHelper/Balloon"
balloon.depth = function(room, entity) return entity.depth or -1 end
--balloon.justification = { 0.0, 0.18 }

balloon.fieldInformation = {
    
}

balloon.placements = {
    name = "balloon",
    data = {
        spriteXMLTag= "ChroniaHelper_balloon",
        depth = -1,
        collider = "r,16,16,-8,-8",
        floatPhase = 0,
        floatScale = 0,
        popSound = "event:/game/general/diamond_touch",
        superBounce = false,
        oneUse = false
    }
}

--[[
function balloon.rectangle(room, entity, viewport)
    return utils.rectangle(entity.x, entity.y - 4, 15, 17)
end
]]

function balloon.texture(room, entity)
    return "objects/ChroniaHelper/balloon/idle00"
end

return balloon
