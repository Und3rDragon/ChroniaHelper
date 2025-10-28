function table.isEmptyTable(obj)
    if (obj == nil) then
        return nil
    end
    return type(obj) == "table" and next(obj) == nil
end

function table.createKeyOrderMap(tbl, customOrder)
    local orderMap = {}
    local index = 1
    
    -- 如果有自定义顺序，按自定义顺序创建映射
    if customOrder then
        for _, key in ipairs(customOrder) do
            if tbl[key] ~= nil then
                orderMap[key] = index
                index = index + 1
            end
        end
    else
        -- 否则按遍历顺序创建映射（Lua不保证顺序，但实际测试中通常保持插入顺序）
        for k, v in pairs(tbl) do
            orderMap[k] = index
            index = index + 1
        end
    end
    
    return orderMap
end

function table.getOrderedKeys(tbl, customOrder)
    local orderMap = createKeyOrderMap(tbl, customOrder)
    local result = {}
    
    -- 创建一个临时数组来排序
    local temp = {}
    for key, order in pairs(orderMap) do
        table.insert(temp, {key = key, order = order})
    end
    
    -- 按顺序排序
    table.sort(temp, function(a, b)
        return a.order < b.order
    end)
    
    -- 提取排序后的key
    for _, item in ipairs(temp) do
        table.insert(result, item.key)
    end
    
    return result
end

-- 更直接的版本：直接返回顺序映射表
function table.getKeyOrderMapping(tbl, customOrder)
    local orderMap = {}
    
    if customOrder then
        -- 使用自定义顺序
        for order, key in ipairs(customOrder) do
            if tbl[key] ~= nil then
                orderMap[key] = order
            end
        end
    else
        -- 使用遍历顺序（不保证完全一致，但通常保持插入顺序）
        local index = 1
        for k, v in pairs(tbl) do
            orderMap[k] = index
            index = index + 1
        end
    end
    
    return orderMap
end

return table