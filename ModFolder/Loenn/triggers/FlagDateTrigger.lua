local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    name = "ChroniaHelper/FlagDateTrigger",
    placements =
    {
        name = "FlagDateTrigger",
        data =
        {
            requirements = "month,1,12;date,1,31",
            setTrueFlags = "flag1, flag2",
            setFalseFlags = "flag3, flag4",
            enterIfFlag = "",
            activeOnStay = true,
            levelDeath = "-1",
            totalDeath = "-1",
            enterMode = "Any",
            enterDelay = 0,
            leaveMode = "Any",
            leaveDelay = 0,
            onlyOnce = false,
            leaveReset = false,
            displayParameters = false,
        }
    },
    fieldInformation =
    {
        enterMode = fieldOptions.enterMode,
        enterDelay =
        {
            minimumValue = 0
        },
        leaveMode = fieldOptions.leaveMode,
        leaveDelay =
        {
            minimumValue = 0
        },
        flag = {
            allowEmpty = false,
        },
        requirements = {
            fieldType = "list",
            elementSeparator = ";",
            elementOptions = {
                options = { "year", "month", "day", "hour", "minute", "second", "millisecond" },
                editable = true,
            },
        },
        setFalseFlags = {
            fieldType = "list",
            elementSeparator = ",",
        },
        setTrueFlags = {
            fieldType = "list",
            elementSeparator = ",",
        },
        enterIfFlag = {
            fieldType = "list",
            elementSeparator = ",",
        },
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "requirements",
        "setFalseFlags",
        "setTrueFlags",
        "enterIfFlag",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "leaveMode",
        "leaveDelay",
        "onlyOnce",
        "leaveReset",
        "activeOnStay",
    },
    triggerText = function(room, entity)
        local req = string.gsub(entity.requirements, ";", "\n")

        if entity.displayParameters then
            return "Flag Date\n" .. req .."\nTrue = " .. entity.setTrueFlags .. "\nFalse = " .. entity.setFalseFlags
        else
            return "Flag Date"
        end
    end
}