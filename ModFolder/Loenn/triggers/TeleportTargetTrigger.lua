local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    name = "ChroniaHelper/TeleportTargetTrigger",
    placements =
    {
        name = "TeleportTargetTrigger",
        data =
        {
            ifFlag = "",
            targetRoom = "",
            targetId = "",
            positionPoint = "TopLeft",
            exitVelocityX = 0,
            exitVelocityY = 0,
            dreaming = "None",
            firstLevel = "None",
            levelDeath = "-1",
            totalDeath = "-1",
            enterMode = "Any",
            enterDelay = 0,
            enterSound = "",
            playerFacing = "None",
            resetDashes = true,
            clearState = true,
            screenWipe = false,
            positionOffset = true
        }
    },
    fieldInformation =
    {
        targetRoom =
        {
            editable = true
        },
        positionPoint = fieldOptions.positionPoint,
        dreaming = fieldOptions.boolMode,
        firstLevel = fieldOptions.boolMode,
        enterMode = fieldOptions.enterMode,
        enterDelay =
        {
            minimumValue = 0
        },
        playerFacing = fieldOptions.playerFacing
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "ifFlag",
        "targetRoom",
        "targetId",
        "positionPoint",
        "exitVelocityX",
        "exitVelocityY",
        "dreaming",
        "firstLevel",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "enterSound",
        "playerFacing",
        "resetDashes",
        "clearState",
        "screenWipe",
        "positionOffset"
    }
}