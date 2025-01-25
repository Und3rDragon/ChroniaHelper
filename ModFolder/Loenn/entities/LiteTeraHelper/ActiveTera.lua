local teraEnum = require("mods").requireFromPlugin("libraries.enum")
local activeTera = {}

activeTera.name = "ChroniaHelper/activeTera"
activeTera.depth = 0
activeTera.texture = "ChroniaHelper/objects/tera/Block/Any"
activeTera.placements = {
	name = "TeraLite - Active Tera",
    data = {
        active = true,
        tera = "Normal",
        onlyOnce = true,
    }
}

activeTera.fieldInformation = {
    tera = {
        fieldType = "anything",
        options = teraEnum.teraType,
        editable = false
    }
}

return activeTera