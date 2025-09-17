local entities = require("entities")
local utils = require("utils")

local function tableContains(t, item)
    for _, v in ipairs(t) do
        if v == item then
            return true
        end
    end
    return false
end

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
        -- 获取新实体（replaceWith）的默认属性模板（不含 x/y/width/height）
        local defaultData = getPlacementData({allowPreserving = false}, handler)

        for _, entity in ipairs(room.entities) do
            if entity._name == entityName then
                -- 1. 修改实体类型
                entity._name = replaceWith

                -- 2. 收集旧实体的所有非敏感属性名（准备后续判断是否要保留）
                local removeAttrs = {}
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
                        table.insert(removeAttrs, key)
                    end
                end

                -- 3. 决定每个属性是否保留 or 被替换
                --    同时标记哪些属性不应被删除（即要保留）
                local keepAttrs = {}  -- 记录不应删除的属性名

                -- 遍历新实体的所有默认属性（即可能要设置的属性）
                for propName, defaultValue in pairs(defaultData) do
                    -- 检查旧实体是否已有该属性
                    local hadSameProp = entity[propName] ~= nil

                    if hadSameProp then
                        -- 判断是否应该用新值替换
                        local shouldReplace = true

                        if args.allowPreserving then
                            -- 如果开启了 preserve，则看用户是否勾选了 replace_propName
                            shouldReplace = args[getChangePropName(propName)] ~= false
                        end

                        if shouldReplace then
                            -- 替换为新值（用户输入 or 默认值）
                            entity[propName] = args[propName]  -- 注意：这里依赖 args[propName] 已设置
                        else
                            -- 不替换 → 保留旧值
                            table.insert(keepAttrs, propName)
                        end
                    else
                        -- 旧实体没有这个属性 → 直接设置新值（新增属性）
                        entity[propName] = args[propName]
                    end

                    -- 无论是否 hadSameProp，只要设置了值，就不该被 removeAttrs 删除
                    table.insert(keepAttrs, propName)
                end

                -- 4. 清理：只删除那些“不在 keepAttrs 中”的旧属性
                for _, key in ipairs(removeAttrs) do
                    -- 如果这个属性不在保留列表中，则删除
                    if not tableContains(keepAttrs, key) then
                        entity[key] = nil
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