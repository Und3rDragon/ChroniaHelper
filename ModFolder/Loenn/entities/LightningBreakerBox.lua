-- 原版电箱（lightningBlock）的可调版本。
-- 字段与画法抄自 Lönn 自带 src/entities/lightning_breaker_box.lua，
-- 额外只多了 hits / bloomPulse / bloomBreak / bloomRest / burstColor / burstColor2 / fenceColor / fenceColor2。

local drawableSprite = require("structs.drawable_sprite")

local customLightbox = {}

customLightbox.name = "ChroniaHelper/LightningBreakerBoxWithNoBloomEffect"
customLightbox.depth = -10550
customLightbox.texture = "objects/breakerBox/Idle00"
customLightbox.fieldInformation = {
    hits = {
        fieldType = "integer",
    },
    music_progress = {
        fieldType = "integer",
    },
    burstColor = {
        fieldType = "color",
    },
    burstColor2 = {
        fieldType = "color",
    },
    fenceColor = {
        fieldType = "color",
    },
    fenceColor2 = {
        fieldType = "color",
    },
}

customLightbox.fieldOrder = {
    "hits",
    "bloomPulse",
    "bloomBreak",
    "bloomRest",
    "burstColor",
    "burstColor2",
    "fenceColor",
    "fenceColor2",
    "flipX",
    "flag",
    "music",
    "music_progress",
    "music_session"
}

customLightbox.placements = {
    name = "default",
    data = {
        hits = 2,
        bloomPulse = 1.2,
        bloomBreak = 1.5,
        bloomRest = 1.0,
        burstColor = "FFFC75",
        burstColor2 = "6BFFFF",
        fenceColor = "B9FEFE",
        fenceColor2 = "FFF263",
        flipX = false,
        flag = false,
        music = "",
        music_progress = -1,
        music_session = false
    }
}

function customLightbox.scale(room, entity)
    local scaleX = entity.flipX and -1 or 1

    return scaleX, 1
end

function customLightbox.justification(room, entity)
    local flipX = entity.flipX

    return flipX and 0.75 or 0.25, 0.25
end

return customLightbox
