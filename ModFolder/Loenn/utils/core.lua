local array = require("mods").requireFromPlugin("utils.array")

local core = { }

function core.fieldCopy(fieldTable, obj)
    local isArray = array.isArray(obj)
    if not(isArray) then
        obj = { obj }
    end
    for index, element in ipairs(obj) do
        local isArrayPlacements = array.isArray(element.placements)
        for fieldName, fieldValue in pairs(fieldTable) do
            if (fieldValue.data ~= nil) then
                if isArrayPlacements then
                    for placementsIndex = 1, #element.placements do
                        element.placements[placementsIndex].data[fieldName] = fieldValue.data
                    end
                else
                    element.placements.data[fieldName] = fieldValue.data
                end
            end
            if (fieldValue.info ~= nil) then
                element.fieldInformation[fieldName] = fieldValue.info
            end
        end
    end
end

return core