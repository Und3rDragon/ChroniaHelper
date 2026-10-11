local customPuffer = {}

customPuffer.name = "ChroniaHelper/CustomPuffer"
customPuffer.placements = {
    {
        name = "right",
        data = {
            right = true,
            static = false,
            alwaysShowOutline = true,
            pushAnyDir = false,
            oneUse = false,
            angle = 0,
            radius = 32,
            launchSpeed = "280",
            respawnTime = 2.5,
            sprite = "pufferFish",
            deathFlag = "",
            holdable = false,
            outlineColor = "ffffffff",
            returnToStart = true,
            holdFlip = false,
            boostMode = "SetSpeed",
            legacyBoost = false,
            absoluteVector = false,
            tangible = true,
            renderEye = true,
            -- new params
            colliders = "r,12,10,-6,-5",
            playerColliders = "r,14,12,-7,-7",
            happySprite = "pufferFish",
            waveFrequency = 0.5,
            waveOffset = 0,
            bounceWiggleDuration = 0.6,
            bounceWiggleFrequency = 2.5,
            inflateWiggleDuration = 0.6,
            inflateWiggleFrequency = 2,
            overrideOutline = 0,
            happyFlag = "happy",
            customizeHappyState = false,
            customizeSound = false,
            pufferBoopSound = "event:/new_content/game/10_farewell/puffer_boop",
            pufferExplodeSound = "event:/new_content/game/10_farewell/puffer_splode",
            pufferShrinkSound = "event:/new_content/game/10_farewell/puffer_shrink",
            pufferReturnSound = "event:/new_content/game/10_farewell/puffer_return",
            pufferExpandSound = "event:/new_content/game/10_farewell/puffer_expand",
            pufferReformSound = "event:/new_content/game/10_farewell/puffer_reform",
        }
    },
    {
        name = "left",
        data = {
            right = false,
            static = false,
            alwaysShowOutline = true,
            pushAnyDir = false,
            oneUse = false,
            angle = 0,
            radius = 32,
            launchSpeed = 280,
            respawnTime = 2.5,
            sprite = "pufferFish",
            deathFlag = "",
            holdable = false,
            outlineColor = "ffffffff",
            returnToStart = true,
            holdFlip = false,
            boostMode = "SetSpeed",
            legacyBoost = false,
            absoluteVector = false,
            tangible = true,
            renderEye = true,
            -- new params
            colliders = "r,12,10,-6,-5",
            playerColliders = "r,14,12,-7,-7",
            happySprite = "pufferFish",
            waveFrequency = 0.5,
            waveOffset = 0,
            bounceWiggleDuration = 0.6,
            bounceWiggleFrequency = 2.5,
            inflateWiggleDuration = 0.6,
            inflateWiggleFrequency = 2,
            overrideOutline = 0,
            happyFlag = "happy",
            customizeHappyState = false,
            customizeSound = false,
            pufferBoopSound = "event:/new_content/game/10_farewell/puffer_boop",
            pufferExplodeSound = "event:/new_content/game/10_farewell/puffer_splode",
            pufferShrinkSound = "event:/new_content/game/10_farewell/puffer_shrink",
            pufferReturnSound = "event:/new_content/game/10_farewell/puffer_return",
            pufferExpandSound = "event:/new_content/game/10_farewell/puffer_expand",
            pufferReformSound = "event:/new_content/game/10_farewell/puffer_reform",
        }
    }
}
customPuffer.fieldInformation = {
    radius = {
        fieldType = "integer",
    },
    outlineColor = {
        fieldType = "color",
        useAlpha = true,
    },
    boostMode = {
        editable = false,
        options = {
            ["Set Speed"] = "SetSpeed",
            ["Redirect Speed"] = "RedirectSpeed",
            ["Redirect + Add Speed"] = "AddRedirectSpeed",
        }
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
}

function customPuffer.ignoredFields(entity)
    local base = {"_name", "_x", "x", "y", "_y"}

    local soundAttrs = {"pufferBoopSound", "pufferExplodeSound",
        "pufferShrinkSound", "pufferReturnSound", "pufferExpandSound", "pufferReformSound"}
    
    local happyAttrs = {"happyFlag", "happySprite"}

    if not entity.customizeSound then
        for _, i in pairs(soundAttrs) do
            table.insert(base, i)
        end
    end

    if not entity.customizeHappyState then
        for _, i in pairs(happyAttrs) do
            table.insert(base, i)
        end
    end

    return base
end

customPuffer.depth = 0
customPuffer.texture = "objects/puffer/idle00"

function customPuffer.scale(room, entity)
    local right = entity.right

    return right and 1 or -1, 1
end

--return customPuffer
