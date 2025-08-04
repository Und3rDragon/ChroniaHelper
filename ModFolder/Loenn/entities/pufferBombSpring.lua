local springDepth = -8501
local springTexture = "objects/spring/00"

local springUp = {}

springUp.name = "ChroniaHelper/PufferBombSpringUp"
springUp.depth = springDepth
springUp.justification = {0.5, 1.0}
springUp.texture = springTexture
springUp.placements = {
    name = "up",
    data = {
        sprite = "pufferBombSpring",
        playerCanUse = true,
        renderOutline = true,
    }
}

function springUp.flip(room, entity, horizontal, vertical)
    if vertical then
        entity._name = "ChroniaHelper/PufferBombSpringDown"
    end

    return vertical
end

function springUp.rotate(room, entity, direction)
    if direction > 0 then
        entity._name = "ChroniaHelper/PufferBombSpringRight"

    else
        entity._name = "ChroniaHelper/PufferBombSpringLeft"
    end

    return true
end

local springRight = {}

springRight.name = "ChroniaHelper/PufferBombSpringRight"
springRight.depth = springDepth
springRight.justification = {0.5, 1.0}
springRight.texture = springTexture
springRight.rotation = math.pi / 2
springRight.placements = {
    name = "right",
    data = {
        sprite = "pufferBombSpring",
        playerCanUse = true,
        renderOutline = true,
    }
}

function springRight.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity._name = "ChroniaHelper/PufferBombSpringLeft"
    end

    return horizontal
end

function springRight.rotate(room, entity, direction)
    if direction < 0 then
        entity._name = "ChroniaHelper/PufferBombSpringUp"
    else
        entity._name = "ChroniaHelper/PufferBombSpringDown"
    end

    return direction < 0
end

local springLeft = {}

springLeft.name = "ChroniaHelper/PufferBombSpringLeft"
springLeft.depth = springDepth
springLeft.justification = {0.5, 1.0}
springLeft.texture = springTexture
springLeft.rotation = -math.pi / 2
springLeft.placements = {
    name = "left",
    data = {
        sprite = "pufferBombSpring",
        playerCanUse = true,
        renderOutline = true,
    }
}

function springLeft.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity._name = "ChroniaHelper/PufferBombSpringRight"
    end

    return horizontal
end

function springLeft.rotate(room, entity, direction)
    if direction > 0 then
        entity._name = "ChroniaHelper/PufferBombSpringUp"
    else
        entity._name = "ChroniaHelper/PufferBombSpringDown"
    end

    return direction > 0
end

local springDown = {}

springDown.name = "ChroniaHelper/PufferBombSpringDown"
springDown.depth = springDepth
springDown.justification = {0.5, 1.0}
springDown.texture = springTexture
springDown.rotation = math.pi
springDown.placements = {
    name = "down",
    data = {
        sprite = "pufferBombSpring",
        playerCanUse = true,
        renderOutline = true,
    }
}

function springDown.flip(room, entity, horizontal, vertical)
    if vertical then
        entity._name = "ChroniaHelper/PufferBombSpringUp"
    end

    return vertical
end

function springDown.rotate(room, entity, direction)
    if direction > 0 then
        entity._name = "ChroniaHelper/PufferBombSpringLeft"
    else
        entity._name = "ChroniaHelper/PufferBombSpringRight"
    end

    return true
end

return {
    springUp,
    springRight,
    springLeft,
    springDown
}