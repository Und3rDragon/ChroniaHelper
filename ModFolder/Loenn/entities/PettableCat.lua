local cat = {}

cat.associatedMods = { "HonlyHelper", "ChroniaHelper" }

cat.name = "ChroniaHelper/PettableCat"
cat.depth = function(entity) return entity.depth or 0 end
cat.placements = {
    name = "default",
    data = {
        interactorY = -4,
        catSpriteXML = "ChroniaHelper_Fox",
        petterSpriteXML = "HonlyHelper_CatPetter",
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
    catSpriteXML = {
        options = {
            "ChroniaHelper_Fox",
            "ChroniaHelper_Cat",
        },
        editable = true,
    },
    petterSpriteXML = {
        options = {
            "HonlyHelper_CatPetter",
            "ChroniaHelper_CatPetter",
        },
        editable = true,
    },
}

cat.sprite = function(room, entity)
    local s = require("structs.drawable_sprite").fromTexture("ChroniaHelper/PettableFox/idle00", entity)

    s:setJustification(0.5,1)

    return s
end

return cat