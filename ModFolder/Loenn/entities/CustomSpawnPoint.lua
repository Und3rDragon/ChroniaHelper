local customSpawnPoint = {}

customSpawnPoint.name = "ChroniaHelper/CustomSpawnPoint"
customSpawnPoint.depth = 0
customSpawnPoint.justification = {0.5, 1.0}
customSpawnPoint.placements = {
    name = "normal",
    data = {
        isDefaultSpawn = false,
        cameraOffsetX = 0,
        cameraOffsetY = 0,
        setFlag = "",
        playerFacingRight = true,
        autoChangeRespawn = true,
        changeRespawnZoneWidth = 32,
        changeRespawnZoneHeight = 48,
    }
}

function customSpawnPoint.sprite(room, entity)
    local sprites = {}
    local playerSprite = require("structs.drawable_sprite").fromTexture("characters/player/sitDown00", entity)
    local rectangle = require("structs.drawable_rectangle").fromRectangle("bordered", entity.x - entity.changeRespawnZoneWidth / 2, entity.y - entity.changeRespawnZoneHeight, entity.changeRespawnZoneWidth, entity.changeRespawnZoneHeight, {1,1,1,0.1}, {1,1,1,0.4})

    playerSprite:setJustification(0.5, 1)

    table.insert(sprites, playerSprite)

    if entity.autoChangeRespawn then
        table.insert(sprites, rectangle)
    end

    return sprites
end

customSpawnPoint.selection = function(room, entity)
	return utils.rectangle(entity.x - 6, entity.y - 16, 12, 16)
end

return customSpawnPoint
