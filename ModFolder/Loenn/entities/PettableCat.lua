local cat = {}

cat.associatedMods = { "HonlyHelper", "ChroniaHelper" }

cat.name = "ChroniaHelper/PettableCat"
cat.depth = function(entity) return entity.depth or 0 end
cat.placements = {
    name = "default",
    data = {
        catSpriteXML = "ChroniaHelper_Cat",
        petterSpriteXML = "ChroniaHelper_CatPetter",
        catPetSound = "event:/HonlyHelper/catsfx",
        petCatFlag = "CatHasBeenPet",
        catGroup = -1,
        depth = 0,
    }
}

cat.fieldInformation = {
    depth = require("mods").requireFromPlugin("helpers.field_options").depths,
    catGroup = {
        fieldType = "integer",
    },
}

cat.sprite = function(room, entity)
    return require("structs.drawable_sprite").fromTexture("characters/HonlyHelper/pettableCat/spoons_idle00", entity):addPosition(4,4)
end

return cat