local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    name = "ChroniaHelper/TeleportPositionTrigger",
    placements =
    {
        name = "TeleportPositionTrigger",
        data =
        {
            ifFlag = "",
            targetRoom = "",
            targetPositionX = 0,
            targetPositionY = 0,
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
            screenWipe = false
        }
    },
    fieldInformation =
    {
        targetRoom =
        {
            editable = true
        },
        targetPositionX =
        {
            fieldType = "integer",
            minimumValue = - 1
        },
        targetPositionY =
        {
            fieldType = "integer",
            minimumValue = - 1
        },
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
        "targetPositionX",
        "targetPositionY",
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
        "screenWipe"
    }
}