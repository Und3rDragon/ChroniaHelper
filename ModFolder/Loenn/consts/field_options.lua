local fieldOptions = { }

--[[
local fieldOptions = require("mods").requireFromPlugin("helpers.field_options")
]]

fieldOptions.boolMode =
{
    options =
    {
        "None",
        "True",
        "False"
    },
    editable = false
}

fieldOptions.enterMode = {
    options =
    {
        "None",
        "Top",
        "Right",
        "Bottom",
        "Left",
        "TopLeft",
        "TopRight",
        "BottomLeft",
        "BottomRight",
        "Horizontal",
        "Vertical",
        "Coordinate",
        "ForwardSlash",
        "BackSlash",
        "Slash",
        "Any"
    },
    editable = false
}

fieldOptions.leaveMode = fieldOptions.enterMode

fieldOptions.playerFacing =
{
    options =
    {
        "None",
        "Left",
        "Right"
    },
    editable = false
}

fieldOptions.positionMode =
{
    options =
    {
        "NoEffect",
        "LeftToRight",
        "RightToLeft",
        "TopToBottom",
        "BottomToTop",
        "HorizontalCenter",
        "VerticalCenter"
    },
    editable = false
}

fieldOptions.positionPoint =
{
    options =
    {
        "TopLeft",
        "TopCenter",
        "TopRight",
        "CenterLeft",
        "Center",
        "CenterRight",
        "BottomLeft",
        "BottomCenter",
        "BottomRight"
    },
    editable = false
}

return fieldOptions