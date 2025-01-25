local table = require("mods").requireFromPlugin("utils.table")

local array = { }

function array.isArray(obj)
    if (obj == nil) then
        return nil
    end
    if type(obj) ~= "table" then
        return false
    end
    if (table.isEmptyTable(obj)) then
        return true
    else
        return #obj > 0
    end
end

function array.add(array, ...)
    for index, element in ipairs( { ...}) do
        table.insert(array, element)
    end
end

function array.indexOf(array, find)
    for index, element in pairs(array) do
        if element == find then
            return index
        end
    end
end

return array