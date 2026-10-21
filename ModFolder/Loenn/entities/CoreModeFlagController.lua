local controller = {}

controller.name = "ChroniaHelper/CoreModeFlagController"
controller.depth = 0
controller.texture = "ChroniaHelper/LoennIcons/Flag"
controller.placements = {
    name = "controller",
    data = {
        coreMode = 0,
        flag = "flag",
    }
}

controller.fieldInformation = {
    coreMode = {
        fieldType = "integer",
        options = {
            ["None"] = 0,
            ["Cold"] = 1,
            ["Hot"] = 2,
        },
        editable = false,
    },
}

return controller
