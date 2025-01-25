local bubblePushField = {}

bubblePushField.name        = "ChroniaHelper/BubbleField"
bubblePushField.depth       = 0
bubblePushField.placements  = {
    name = "Bubble Field (Customizable)",
    data = {
        strength        = 1.0,
        direction       = "Right",
        flag            = "bubble_push_field",
        activationMode  = "Always",
        liftOffOfGround = true,
        force           = true,
        particleDirs    = "particles/ChroniaHelper/bubble_a,particles/ChroniaHelper/bubble_b",
        width           = 8,
        height          = 8,
        noParticles     = false,

    }
}

bubblePushField.fieldInformation = {
    direction = {
        options = {
            "Up",
            "Down",
            "Left",
            "Right",
        },
        editable = false,
    },
    activationMode = {
        options = {
            "Always",
            "OnlyWhenFlagActive",
            "OnlyWhenFlagInactive",
        },
        editable = false,
    },
    particleDirs ={
        fieldType = "list",
    },
}

bubblePushField.fillColor   = { 0.0, 0.2, 1.0, 0.4 }
bubblePushField.borderColor = { 1.0, 1.0, 1.0, 0.5 }

return bubblePushField
