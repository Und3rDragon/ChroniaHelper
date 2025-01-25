local entity = {}

entity.name = "ChroniaHelper/DecreaseRefill"
entity.placements = {
    name = "DecreaseRefill",
    data = {
        sprites = "objects/ChroniaHelper/decreaseRefill/",
        depth = 8999,
        respawnTimer = 2.5,
        oneUse = false,
        dashes = 1,
        freezeFrameLength = 0.05,
        decreaseStamina = false,
        clearAllDashes = false,
        tradeDashesToStamina = false,
        frameRates = "12, 12",
    }
}
entity.depth = function(room,entity) return entity.depth or 8999 end
entity.fieldInformation = {
    dashes ={
        fieldType="integer",
    },
    sprites = require("mods").requireFromPlugin("libraries.vivUtilsMig").getDirectoryPathFromFile(true),
}
entity.fieldOrder={
    "sprites",
    "depth",
    "dashes",
    "clearAllDashes",
    "respawnTimer",
    "oneUse",
    "decreaseStamina",
    "tradeDashesToStamina",
    "freezeTimeLength",
    "frameRates",
}
entity.texture = function(room, entity)
    --return entity.sprites
    return "objects/ChroniaHelper/decreaseRefill/idle00"
end


return entity