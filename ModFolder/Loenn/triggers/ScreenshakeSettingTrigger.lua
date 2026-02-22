local t = {}

t.name = "ChroniaHelper/ScreenshakeSettingTrigger"

t.placements = {
    name = "normal",
    data = {
        width = 16,
        height = 16,
        value = 0,
    }
}

t.fieldInformation = {
    value = {
        options = {
            ["Off"] = 0,
            ["Half"] = 1,
            ["On"] = 2,
        },
        editable = false,
    },
}

return t