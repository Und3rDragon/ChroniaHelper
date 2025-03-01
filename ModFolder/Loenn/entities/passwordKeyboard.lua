local entity = {}

entity.name = "ChroniaHelper/PasswordKeyboard"
entity.texture = "PasswordKeyboard/keyboard"
entity.justification = { 0.5, 1.0 }
entity.placements = {
    name = "normal",
    data = {
        width = 16,
        height = 16,

        mode = 0,
        flagToEnable = "",
        password = "",
        rightDialog = "rightDialog",
        wrongDialog = "wrongDialog",
        caseSensitive = false,
        useTimes = -1
    }
}

entity.fieldInformation = {
    mode = {
        fieldType = "integer",
        options = {
            ["Exclusive"] = 0,
            ["Normal"] = 1,
            ["OutputFlag"] = 2
        },
        editable = false
    },
    useTimes = {
        fieldType = "integer"
    }
}

return entity
