local drawableSprite = require("structs.drawable_sprite")

local theoCrystal = {}

theoCrystal.name = "ChroniaHelper/CustomTheoCrystal"
theoCrystal.depth = 100
theoCrystal.placements = {
    name = "theo_crystal",
    data = {
        depth = 100,
        birdTutorialTitle = "tutorial_hold",
        birdTutorialText = "Grab",
        tutorialIconOffsetX = 0,
        tutorialIconOffsetY = -24,
        hitSeekerSound = "event:/game/05_mirror_temple/crystaltheo_hit_side",
        hitSideSound = "event:/game/05_mirror_temple/crystaltheo_hit_side",
        hitGroundSound = "event:/game/05_mirror_temple/crystaltheo_hit_ground",
        textureXML = "theo_crystal",
        holdableCooldown = 0.1,
        holdableWidth = 16,
        holdableHeight = 22,
        holdableX = -8,
        holdableY = -16,
        liftSpeedGraceTime = 0.1,
        vertexLightOffsetX = 0,
        vertexLightOffsetY = 0,
        hexLightColor = "ffffffff",
        vertexLightStartFade = 32,
        vertexLightEndFade = 64,
        slowFall = false,
        slowRun = true,
        mirrorReflection = true,
        displayTutorial = false,
    }
}

theoCrystal.fieldInformation = {
    depth = {
        options = require("mods").requireFromPlugin("consts.depths"),
        editable = true,
    },
    vertexLightStartFade = {
        fieldType = "integer",
    },
    vertexLightStartFade = {
        fieldType = "integer",
    },
    hexLightColor = {
        useAlpha = true,
    },
}

-- Offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "characters/theoCrystal/idle00"

function theoCrystal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end

return theoCrystal