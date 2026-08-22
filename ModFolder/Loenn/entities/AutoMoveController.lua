local controller = {}

controller.name = "ChroniaHelper/AutoMoveController"
controller.depth = 0
controller.texture = "ChroniaHelper/LoennIcons/OperCodes"
controller.placements = {
    name = "controller",
    data = {
        commands = "jump;0.5;jump",
        playFlag = "autoMove",
    }
}

controller.fieldInformation = {
    commands = {
        fieldType = "list_scroll",
        elementSeparator = ";",
        elementOptions = {
            fieldType = "list_scroll",
        },
    },
}

return controller
