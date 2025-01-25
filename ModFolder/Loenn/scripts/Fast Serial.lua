local script = {}

script.name = "fastSerial"
script.displayName = "Fast Name Array"

--script.tooltip = "Converts VivHelper Spinners to Chronia Helper Spinners"

script.tooltips = {
    objectName = "The name displayed on top of the entity or trigger window that looks like 'xxxHelper/xxx'",
    targetAttribute = "The option name you wanna apply this name array on",
    serialName = "The name prefix for the name array"
}

script.parameters = {
    objectName = "",
    targetAttribute = "",
    serialName = "",
}



--script.fieldOrder = {}

function script.run(room, args)
    local i = 0
    for _, entity in ipairs(room.entities) do
        if entity._name == args.objectName then
            entity[args.targetAttribute] = args.serialName .. i

            --print(require("utils").serialize(entity))
            i = i + 1
        end
    end
    for _, trigger in ipairs(room.triggers) do
        if trigger._name == args.objectName then
            trigger[args.targetAttribute] = args.serialName .. i

            --print(require("utils").serialize(entity))
            i = i + 1
        end
    end
end

return script