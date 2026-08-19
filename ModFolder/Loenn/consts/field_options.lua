local fieldOptions = { }

--[[
local fieldOptions = require("mods").requireFromPlugin("consts.field_options")
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

fieldOptions.generalSetup = {
    fieldType = "integer",
    options = {
        ["On Level Load"] = 0,
        ["Always Set"] = 1,
        ["On Scene Start"] = 2,
        ["On Scene End"] = 3,
        ["On Interval"] = 4,
        ["On Player Die"] = 5,
        ["On Player Respawn"] = 6,
        ["On Entity Added"] = 7,
        ["On Entity Removed"] = 8,
        ["On Flags Enable"] = 9,
        ["On Chronia Expression Enable"] = 10,
        ["On Frost Session Expression Enable"] = 11,
        ["On Chronia Flag Logic Expression Enable"] = 12,
        ["On Flags Disable"] = 13,
        ["On Chronia Expression Disable"] = 14,
        ["On Frost Session Expression Disable"] = 15,
        ["On Chronia Flag Logic Expression Disable"] = 16,
        ["On Flags State"] = 17,
        ["On Chronia Expression State"] = 18,
        ["On Frost Session Expression State"] = 19,
        ["On Chronia Flag Logic Expression State"] = 20
    },
    editable = false,
}

return fieldOptions