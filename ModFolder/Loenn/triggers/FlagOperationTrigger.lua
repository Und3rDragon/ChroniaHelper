local fieldOptions = require("mods").requireFromPlugin("consts.field_options")

--[[
return {
    name = "ChroniaHelper/FlagOperationTrigger",
    placements =
    {
        name = "FlagOperationTrigger",
        data =
        {
            flag = "targetFlag",
            operation = 0,
            dataName = "value",
            dataValue = "0",
            numeralExpression = "(#value + 3)^2 - 5",
        }
    },
    fieldInformation =
    {
        operation = {
            options = {
                ["add data"] = 0,
                ["remove data"] = 1,
                ["calculate expression (numeral)"] = 2,
                ["flag if contains"] = 3,
                ["flag if not contains"] = 4,
            },
            editable = false,
        },
        flag = {
            allowsEmpty = false,
        },
        dataName = {
            allowsEmpty = false,
        },
        dataValue = {
            allowsEmpty = false,
        },
        numeralExpression = {
            allowsEmpty = false,
        },
    }
}
]]
