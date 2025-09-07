local fakeTilesHelper = require("helpers.fake_tiles")

local fakeWall = {}

fakeWall.name = "ChroniaHelper/CustomFakeWall"
fakeWall.depth = function(entity,room) return entity.depth end
fakeWall.placements = {
    name = "CustomFakeWall",
    data = {
        tiletype = "3",
        width = 8,
        height = 8,
        depth = -13000,
        audioEvent = "event:/game/general/secret_revealed",
        playReveal = "NotOnTransition",
        recover = false,
        permanent = false,
        playSoundOnEachTrigger = true,
    }
}

fakeWall.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesFg", {1.0, 1.0, 1.0, 0.7})
fakeWall.fieldInformation = function(entity)
    local orig = fakeTilesHelper.getFieldInformation("tiletype")(entity)
    orig["Depth"] ={
        fieldType = "integer",
        options = require("mods").requireFromPlugin("helpers.field_options").depths
    }
    orig['playReveal'] = {
        fieldType = "string",
        options = {"Never","NotOnTransition","Always"},
        editable = false,
    }
    return orig
end

return fakeWall