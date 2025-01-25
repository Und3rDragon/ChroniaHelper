local fieldOptions = require("mods").requireFromPlugin("helpers.field_options")

local baseEntity = { }

baseEntity.fields = {
    width =
    {
        data = 8,
        info =
        {
            fieldType = "integer",
            minimumValue = 0
        }
    },
    height =
    {
        data = 8,
        info =
        {
            fieldType = "integer",
            minimumValue = 0
        }
    }
}

function baseEntity.invoke(entity)
    for index, element in pairs(entity.fieldOrder) do
        if baseEntity.fields[element] then
            entity.placements.data[element] = baseEntity.fields[element].data
            entity.fieldInformation[element] = baseEntity.fields[element].info
        end
    end
end

return baseEntity