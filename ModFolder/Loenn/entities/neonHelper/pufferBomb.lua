-- The source code of this entity is migrated from NeonHelper, which is integrated in City of Broken Dreams
-- The original author is ricky06, code modified by UnderDragon


local pufferBomb = {}

pufferBomb.name = "ChroniaHelper/PufferBomb"
pufferBomb.depth = 0
pufferBomb.texture = "objects/puffer/idle00"
pufferBomb.placements = {        
    name = "Puffer Bomb",
    data = {
        sprite = "pufferBomb",
        rangeIndicator = "bombRange",
        basicColliders = "r,12,10,-6,-5",
        playerColliders = "r,14,12,-7,-7",
        detectCollider = "60,30,-30,0",
        wallbreakCollider = "16,0,0",
        longRange = false,
        alwaysBoost = true,
        ignoreSolids = false,
    }
}

pufferBomb.fieldInformation = {
    basicColliders = {
        fieldType = "list",
        elementSeparator = ";",
        elementOptions = {
            fieldType = "list",
            allowEmpty = false,
        },
        minimumElements = 1,
    },
    playerColliders = {
        fieldType = "list",
        elementSeparator = ";",
        elementOptions = {
            fieldType = "list",
            allowEmpty = false,
        },
        minimumElements = 1,
    },
    detectCollider = {
        fieldType = "list",
        minimumElements = 2,
        maximumElements = 4,
        minimumElements = 1,
    },
    wallbreakCollider = {
        fieldType = "list",
        minimumElements = 1,
        maximumElements = 3,
        minimumElements = 1,
    }
}

return pufferBomb