local drawableSprite = require("structs.drawable_sprite")
local teraEnum = require("mods").requireFromPlugin("libraries.enum")
local teraTouchSwitch = {}

teraTouchSwitch.name = "ChroniaHelper/teraTouchSwitch"
teraTouchSwitch.depth = 2000
teraTouchSwitch.placements = {
	name = "TeraLite - Touch Switch",
    data = {
        tera = "Normal",
    }
}
teraTouchSwitch.fieldInformation = {
    tera = {
        options = teraEnum.teraType,
        editable = false
    }
}

local containerTexture = "objects/touchswitch/container"
function teraTouchSwitch.sprite(room, entity, viewport)
    local tera = entity.tera or "Normal"
    local containerSprite = drawableSprite.fromTexture(containerTexture, entity)
    local iconTexture = "ChroniaHelper/objects/tera/TouchSwitch/" .. tera .. "00"
    local iconSprite = drawableSprite.fromTexture(iconTexture, entity)
    return {containerSprite, iconSprite}
end

return teraTouchSwitch