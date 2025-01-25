function string.firstUpper(str, forceLower)
    if not forceLower then
        return string.upper(string.sub(str, 0, 1)) .. string.sub(str, 2)
    end
    return string.upper(string.sub(str, 0, 1)) .. string.lower(string.sub(str, 2))
end

function string.firstLower(str, forceUpper)
    if not forceUpper then
        return string.lower(string.sub(str, 0, 1)) .. string.sub(str, 2)
    end
    return string.lower(string.sub(str, 0, 1)) .. string.upper(string.sub(str, 2))
end

function string.supply(str, supply, len, front)
    local strLen = string.len(str)
    if strLen >= len then
        return str
    end
    local surplus = len - strLen
    local supplyLen = string.len(supply)
    local temp = ""
    for i = 1, surplus / supplyLen do
        temp = temp .. supply
    end
    local modular = surplus % supplyLen
    if modular ~= 0 then
        temp = temp .. temp.sub(supply, 0, modular)
    end
    return front and temp .. str or str .. temp
end

function string.supplyFront(str, supply, len)
    return string.supply(str, supply, len, true)
end

function string.supplyRear(str, supply, len)
    return string.supply(str, supply, len, false)
end

return string