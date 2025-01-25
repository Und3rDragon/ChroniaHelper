function table.isEmptyTable(obj)
    if (obj == nil) then
        return nil
    end
    return type(obj) == "table" and next(obj) == nil
end

return table