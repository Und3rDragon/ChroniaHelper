local inputFlagController = {}

inputFlagController.name = "ChroniaHelper/InputFlagController"
inputFlagController.depth = -1000000
inputFlagController.texture = "ChroniaHelper/LoennIcons/Flag"

inputFlagController.associatedMods = {"ChroniaHelper", "CommunalHelper"}

inputFlagController.placements = {
    {
        name = "controller",
        data = {
            flags = "",
            ifFlagCondition = "",
            mode = 0,
            delay = 0.0,
            restraints = 0,
            restraintValue = "",
            controllerIDPrefix = "ChroniaHelper_Input_",
            detectCounterID = "inputListenerCounter",
            resetFlags = false,
            resetCounter = false,
            resetDetectSession = false,
            activateByDefault = true,
            activateByGrab = false,
            activateByDash = false,
            activateByJump = false,
            activateByTalk = false,
            activateByCrouchDash = false,
            activateByESC = false,
            activateByPause = false,
            onlyOnHeld = false,
        }
    }
}

inputFlagController.fieldInformation = {
    flags = {
        fieldType = "list",
        elementSeparator = ";",
        elementOptions = {
            fieldType = "list",
        },
    },
    ifFlagCondition = {
        fieldType = "list",
    },
    mode = {
        options = {
            ["Enable Flags"] = 2,
            ["Toggle Flags"] = 0,
            ["Flag With Counter Suffix"] = 1,
            ["Disable Flags"] = 3,
        },
        editable = false,
    },
    restraints = {
        options = {
            ["No Restraints"] = 0,
            ["Usage Equals"] = 1,
            ["Usage Lower"] = 2,
            ["Usage Greater"] = 3,
            ["Usage Equals Or Lower"] = 4,
            ["Usage Equals Or Greater"] = 5,
            ["Usage Within Range"] = 6,
        },
        editable = false,
    },
}

return inputFlagController
