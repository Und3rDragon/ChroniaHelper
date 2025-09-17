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
        -- ��ȡ��ʵ�壨replaceWith����Ĭ������ģ�壨���� x/y/width/height��
        local defaultData = getPlacementData({allowPreserving = false}, handler)

        for _, entity in ipairs(room.entities) do
            if entity._name == entityName then
                -- 1. �޸�ʵ������
                entity._name = replaceWith

                -- 2. �ռ���ʵ������з�������������׼�������ж��Ƿ�Ҫ������
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

                -- 3. ����ÿ�������Ƿ��� or ���滻
                --    ͬʱ�����Щ���Բ�Ӧ��ɾ������Ҫ������
                local keepAttrs = {}  -- ��¼��Ӧɾ����������

                -- ������ʵ�������Ĭ�����ԣ�������Ҫ���õ����ԣ�
                for propName, defaultValue in pairs(defaultData) do
                    -- ����ʵ���Ƿ����и�����
                    local hadSameProp = entity[propName] ~= nil

                    if hadSameProp then
                        -- �ж��Ƿ�Ӧ������ֵ�滻
                        local shouldReplace = true

                        if args.allowPreserving then
                            -- ��������� preserve�����û��Ƿ�ѡ�� replace_propName
                            shouldReplace = args[getChangePropName(propName)] ~= false
                        end

                        if shouldReplace then
                            -- �滻Ϊ��ֵ���û����� or Ĭ��ֵ��
                            entity[propName] = args[propName]  -- ע�⣺�������� args[propName] ������
                        else
                            -- ���滻 �� ������ֵ
                            table.insert(keepAttrs, propName)
                        end
                    else
                        -- ��ʵ��û��������� �� ֱ��������ֵ���������ԣ�
                        entity[propName] = args[propName]
                    end

                    -- �����Ƿ� hadSameProp��ֻҪ������ֵ���Ͳ��ñ� removeAttrs ɾ��
                    table.insert(keepAttrs, propName)
                end

                -- 4. ����ֻɾ����Щ������ keepAttrs �С��ľ�����
                for _, key in ipairs(removeAttrs) do
                    -- ���������Բ��ڱ����б��У���ɾ��
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