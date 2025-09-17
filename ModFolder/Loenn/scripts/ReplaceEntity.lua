local entities = require("entities")
local utils = require("utils")

local script = {
    name = "replaceEntity",
    displayName = "Replace Entity",
    tooltip = "Replace Entity",
    tooltips = {
        type = "The SID of entities that will be impacted by this script",
        replaceWith = "The SID that affected entities will be turned into. Leave blank to not change entity type",
        allowPreserving = "If enabled, extra settings will be added to allow to leave some properties untouched on affected entities.\nUseful when only changing one property of entities in bulk."
    },
    parameters = {
        type = "",
        replaceWith = "",
        allowPreserving = false,
    },
    fieldInformation = {
        type = {
            options = (function ()
                local options = {}
                for name, _ in ipairs(entities.registeredEntities) do
                    table.insert(options, name)
                end

                return options
            end)(), -- todo: don't call this once 'options' accepts functions
            editable = true
        }
    },
    fieldOrder = {"type", "allowPreserving", "replaceWith"}
}

local function getChangePropName(propName)
    return "replace_" .. propName
end

local function getPlacementData(args, handler)
    local all = utils.callIfFunction(handler.placements)
    local _data = (all.default or all[1] or all).data
    local data =  utils.deepcopy(_data)

    data.x = nil
    data.y = nil
    data.width = nil
    data.height = nil

    if args.allowPreserving then
        for key, value in pairs(_data) do
            data[getChangePropName(key)] = false
        end
    end


    return data
end

local function getFieldOrder(args, handler)
    local placementData = getPlacementData({allowPreserving = false}, handler)
    local order = {}

    for key, value in pairs(placementData) do
        table.insert(order, key)
        if args.allowPreserving then
            table.insert(order, getChangePropName(key))
        end
    end

    return order
end

local function getReplaceScriptRunFunction(entityName, replaceWith, handler)
    return function (room, args)
        for _, entity in ipairs(room.entities) do
            if entity._name == entityName then
                entity._name = replaceWith
                
                for key, value in pairs(entity) do
                    print("entity." .. tostring(key) .. " = " .. tostring(value))
                end
                
                 -- 移除非 sensitive 属性
                for key in pairs(entity) do
                    local isSensitive = (key == "_name" or 
                                         key == "_id" or 
                                         key == "_type" or 
                                         key == "x" or 
                                         key == "y" or 
                                         key == "_x" or 
                                         key == "_y" or 
                                         key == "_fromLayer")

                    if not isSensitive then
                        entity[key] = nil  -- 删除该属性
                    end
                end
                
                for propName, _ in pairs(getPlacementData({allowPreserving = false}, handler)) do
                    if args[getChangePropName(propName)] ~= false then
                        entity[propName] = args[propName]
                    end
                end
            end
        end
    end
end

function script.prerun(args, layer, ctx)
    local entityName = args.type
    local replaceWith = args.replaceWith
    if replaceWith == "" then
        replaceWith = entityName
    end

    local entityHandler = entities.registeredEntities[replaceWith]

    local placementData = getPlacementData(args, entityHandler)
    local fieldOrder = getFieldOrder(args, entityHandler)

    local changeHandler = {
        displayName = string.format("Change all %s", entityName),
        parameters = placementData,
        fieldOrder = fieldOrder,
        fieldInformation = entityHandler.fieldInformation,
        run = getReplaceScriptRunFunction(entityName, replaceWith, entityHandler),
    }

    script.scriptsTool.useScript(changeHandler, ctx)
end



return script