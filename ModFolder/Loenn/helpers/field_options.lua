local musics = require("consts.songs")
local ambiences = require("consts.ambient_sounds")

--[[
local fo = require("mods").requireFromPlugin("helpers.field_options")
]]


local fieldOptions = { }

fieldOptions.musics =
{
    options = musics,
    editable = true
}

fieldOptions.ambiences =
{
    options = ambiences,
    editable = true
}

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

fieldOptions.facingMode =
{
    options =
    {
        "None",
        "Left",
        "Right"
    },
    editable = false
}

fieldOptions.enterFrom = {
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

fieldOptions.leaveFrom = fieldOptions.enterFrom

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

fieldOptions.direction =
{
    options =
    {
        "Up",
        "Down",
        "Left",
        "Right"
    },
    editable = false
}

fieldOptions.easeMode =
{
    options =
    {
        "None",
        "Linear",
        "SineIn",
        "SineOut",
        "SineInOut",
        "QuadIn",
        "QuadOut",
        "QuadInOut",
        "CubeIn",
        "CubeOut",
        "CubeInOut",
        "QuintIn",
        "QuintOut",
        "QuintInOut",
        "ExpoIn",
        "ExpoOut",
        "ExpoInOut",
        "BackIn",
        "BackOut",
        "BackInOut",
        "BigBackIn",
        "BigBackOut",
        "BigBackInOut",
        "ElasticIn",
        "ElasticOut",
        "ElasticInOut",
        "BounceIn",
        "BounceOut",
        "BounceInOut"
    },
    editable = false
}

fieldOptions.depths = {
    fieldType = "integer",
    options = {
    ["BGTerrain 10000"] = 10000,
    ["BGMirrors 9500"] = 9500,
    ["BGDecals 9000"] = 9000,
    ["BGParticles 8000"] = 8000,
    ["SolidsBelow 5000"] = 5000,
    ["Below 2000"] = 2000,
    ["NPCs 1000"] = 1000,
    ["TheoCrystal 100"] = 100,
    ["Player 0"] = 0,
    ["Dust -50"] = -50,
    ["Pickups -100"] = -100,
    ["Seeker -200"] = -200,
    ["Particles -8000"] = -8000,
    ["Above -8500"] = -8500,
    ["Solids -9000"] = -9000,
    ["FGTerrain -10000"] = -10000,
    ["FGDecals -10500"] = -10500,
    ["DreamBlocks -11000"] = -11000,
    ["CrystalSpinners -11500"] = -11500,
    ["PlayerDreamDashing -12000"] = -12000,
    ["Enemy -12500"] = -12500,
    ["FakeWalls -13000"] = -13000,
    ["FGParticles -50000"] = -50000,
    ["Top -1000000"] = -1000000,
    ["FormationSequences -2000000"] = -2000000
    },
    editable = true,
}

fieldOptions.positionMode = {
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
}

return fieldOptions