local customSpawnPoint = {}

customSpawnPoint.name = "ChroniaHelper/CustomSpawnPoint"
customSpawnPoint.depth = 0
customSpawnPoint.justification = {0.5, 1.0}
customSpawnPoint.nodeLimits = {0, 2}
customSpawnPoint.placements = {
    name = "normal",
    data = {
        isDefaultSpawn = false,
        playerFacing = 0,

        cameraOffsetX = "",
        cameraOffsetY = "",
        cameraCatchupSpeed = -1,
        setFlag = "",
        
        autoChangeRespawn = true,
        effectZone = "See Tooltip",
        clearFlagsBeforeSet = false,
    }
}

customSpawnPoint.fieldInformation = {
    playerFacing = {
        fieldType = "integer",
        options = {
            ["Disable"] = 0,
            ["Left"] = -1,
            ["Right"] = 1,
        },
        editable = false,
    },
    setFlag = {
        fieldType = "list_scroll",
    },
    effectZone = {
        editable = false,
    },
}

function customSpawnPoint.sprite(room, entity)
    local sprites = {}
    local playerSprite = require("structs.drawable_sprite").fromTexture("characters/player/sitDown00", entity)

    playerSprite:setJustification(0.5, 1)

    table.insert(sprites, playerSprite)

    if entity.nodes ~= nil then
        if #entity.nodes > 0 then
            for _, node in ipairs(entity.nodes) do
                local rec = require("structs.drawable_rectangle").fromRectangle("fill", node.x - 3, node.y - 3, 6, 6, {1,1,1,1})
            
                table.insert(sprites, rec)
            end
        end

        if #entity.nodes >= 2 then
            local p1 = entity.nodes[1]
            local p2 = entity.nodes[2]
            local rectangle = require("structs.drawable_rectangle").fromRectangle("bordered", p1.x, p1.y, p2.x - p1.x, p2.y - p1.y, {1,1,1,0.1}, {1,1,1,0.4})

            table.insert(sprites, rectangle)
        end
    end

    return sprites
end

function customSpawnPoint.selection(room, entity)
    local baseRec = utils.rectangle(entity.x - 6, entity.y - 16, 12, 16)

    local nodeRecs = {}
    if entity.nodes ~= nil then
        if #entity.nodes > 0 then
            for _, node in ipairs(entity.nodes) do
                local nodeRec = utils.rectangle(node.x - 3, node.y - 3, 6, 6)

                table.insert(nodeRecs, nodeRec)
            end
        end
    end

	return baseRec, nodeRecs
end

return customSpawnPoint
