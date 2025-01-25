return {
    triggerText = function(room, entity)
        local base = "Smooth Camera"
        local cam = "(" .. entity.offsetXTo .. "," .. entity.offsetYTo .. ")"
        if entity.modes == "xOnly" then
            cam = "(X to " .. entity.offsetXTo .. ")"
        elseif entity.modes == "yOnly" then
            cam = "(Y to " .. entity.offsetYTo .. ")"
        end
        local f = ""
        if entity.flagControl ~= "disabled" then
            f = " (" .. entity.flagControl .. ": " .. entity.flag .. ")"
        end
        return base .. " " .. cam .. f
    end,
    category = "camera",
    name = "ChroniaHelper/SmoothToOffsetCamera",
    placements =
    {
        name = "SmoothToOffsetCamera",
        data ={
            width = 48,    
            height = 56,
            offsetXTo = 0,
            offsetYTo = 0,
            onlyOnce = false,
            positionMode = "NoEffect",
            x = 40,
            y = 40,
            unit = "offset",
            modes = "normal",
            flag = "flag",
            flagControl = "disabled",
        },
    },
    fieldInformation = {
        positionMode = {
            options = {
                "NoEffect",
                "RightToLeft",
                "LeftToRight",
                "BottomToTop",
                "TopToBottom",
                "VerticalCenter",
                "HorizontalCenter",
            },
            editable = false,
        },
        unit = {
            options = {
                "offset",
                "offsetSquared",
                "tiles",
                "pixels",
            },
            editable = false,
        },
        modes = {
            options = {
                "normal",
                "xOnly",
                "yOnly",
            },
            editable = false,
        },
        flagControl ={
            options = {
                "disabled",
                "flagRequired",
                "flagInverted",
            },
            editable = false,
        },
    },
}