local controller = {}

controller.name = "ChroniaHelper/ForceMovementController"
controller.depth = 0
controller.texture = "ChroniaHelper/LoennIcons/ForceMovement"
controller.placements = {
    name = "controller",
    data = {
        value = 1,
        up = false,
        down = false,
        left = false,
        right = false,
        jump = false,
        dash = false,
        grab = false,
        onlyIfFlag = ""
    }
}

return controller
