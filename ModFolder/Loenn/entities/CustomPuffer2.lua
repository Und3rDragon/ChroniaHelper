local puffer = {}

puffer.name = "ChroniaHelper/CustomPuffer2"
puffer.depth = function(room,entity) return entity.depth or 1 end
puffer.texture = "objects/puffer/idle00"
puffer.placements = {
    {
        name = "left",
        data = {
            depth = 1,
            colliders = "r,12,10,-6,-5",
            playerColliders = "r,14,12,-7,-7",
            spriteXMLTag = "pufferFish",
            waveFrequency = 0.5,
            waveOffset = 0,
            radiusPush = 40,
            radiusDetect = 32,
            radiusBreakWalls = 16,
            scaleX = 1,
            scaleY = 1,
            bounceWiggleDuration = 0.6,
            bounceWiggleFrquency = 2.5,
            inflateWiggleDuration = 0.6,
            inflateWiggleFrequency = 2,
            overrideOutline = 0,
            indicatorColor = "ffffffff",
            right = false
        }
    },
    {
        name = "right",
        data = {
            depth = 1,
            colliders = "r,12,10,-6,-5",
            playerColliders = "r,14,12,-7,-7",
            spriteXMLTag = "pufferFish",
            waveFrequency = 0.5,
            waveOffset = 0,
            radiusPush = 40,
            radiusDetect = 32,
            radiusBreakWalls = 16,
            scaleX = 1,
            scaleY = 1,
            bounceWiggleDuration = 0.6,
            bounceWiggleFrquency = 2.5,
            inflateWiggleDuration = 0.6,
            inflateWiggleFrequency = 2,
            overrideOutline = 0,
            indicatorColor = "ffffffff",
            right = true
        }
    }
}

puffer.fieldInformation = {
    colliders = {
        fieldType = "list",
        elementSeparator = ";",
    },
    playerColliders = {
        fieldType = "list",
        elementSeparator = ";",
    },
    overrideOutline = {
        fieldType = "integer",
        options = {
            ["Default"] = 0,
            ["true"] = 1,
            ["False"] = 2,
        },
        editable = false,
    },
    depth = {
        options = require("mods").requireFromPlugin("consts.depths"),
        editable = true,
    },
    indicatorColor = {
        fieldType = "color",
        useAlpha = true,
    },
}

function puffer.scale(room, entity)
    local right = entity.right

    return right and 1 or -1, 1
end

function puffer.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.right = not entity.right
    end

    return horizontal
end

return puffer