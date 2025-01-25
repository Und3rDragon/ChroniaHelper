local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

return {
    triggerText = function(room, entity)
        local base = "Flag String " .. entity.executor
        if entity.executor == "AddAsPrefix" then
            base = base .. " (" .. entity.baseString .. ")"
        elseif entity.executor == "AddAsSuffix" then
            base = base .. " (" .. entity.baseString .. ")"
        elseif entity.executor == "RemoveFirst" then
            base = base .. " (" .. entity.baseString .. ")"
        elseif entity.executor == "RemoveLast" then
            base = base .. " (" .. entity.baseString .. ")"
        elseif entity.executor == "RemoveAll" then
            base = base .. " (" .. entity.baseString .. ")"
        elseif entity.executor == "ReplaceAll" then
            base = base .. " (" .. entity.baseString .. ") with (" .. entity.newString .. ")"
        elseif entity.executor == "ReplaceFirst" then
            base = base .. " (" .. entity.baseString .. ") with (" .. entity.newString .. ")"
        elseif entity.executor == "ReplaceLast" then
            base = base .. " (" .. entity.baseString .. ") with (" .. entity.newString .. ")"
        end
        return base
    end,
    name = "ChroniaHelper/FlagStringTrigger",
    placements =
    {
        name = "FlagStringTrigger",
        data =
        {
            executor = "AddAsSuffix",
            baseString = "",
            newString = "",
            leaveReset = false,
            onlyOnce = false,
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
        executor = {
            options = {
                "AddAsPrefix",
                "AddAsSuffix",
                "RemoveFirst",
                "RemoveLast",
                "RemoveAll",
                "ReplaceFirst",
                "ReplaceLast",
                "ReplaceAll",
                "Backspace",
                "Delete",
                "Clear"
            },
            editable = false,
        },
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "flag",
        "levelDeath",
        "totalDeath",
        "enterMode",
        "enterDelay",
        "leaveMode",
        "leaveDelay",
        "onlyOnce",
        "leaveReset"
    }
}