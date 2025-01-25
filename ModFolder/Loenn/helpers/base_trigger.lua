local fieldOptions = require("mods").requireFromPlugin("helpers.field_options")

local baseTrigger = { }

baseTrigger.fields = {
    enterFrom =
    {
        data = "Any",
        info = fieldOptions.enterFrom
    },
    flagsForEnter =
    {
        data = ""
    },
    leaveFrom =
    {
        data = "Any",
        info = fieldOptions.leaveFrom
    },
    flagsForLeave =
    {
        data = ""
    },
    onlyOnce =
    {
        data = false
    },
    revertOnLeave =
    {
        data = false
    },
    revertOnDeath =
    {
        data = true
    }
}

function baseTrigger.invoke(trigger)
    for index, element in pairs(trigger.fieldOrder) do
        if baseTrigger.fields[element] then
            trigger.placements.data[element] = baseTrigger.fields[element].data
            trigger.fieldInformation[element] = baseTrigger.fields[element].info
        end
    end
end

return baseTrigger