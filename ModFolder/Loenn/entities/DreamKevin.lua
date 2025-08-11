local dreamKevin = {}

local function rgbaToFloat(r, g, b, a)
    return {r / 255.0, g / 255.0, b / 255.0, a / 255.0}
end

local axesOptions = {
    Both = "both",
    Vertical = "vertical",
    Horizontal = "horizontal"
}

dreamKevin.name = "ChroniaHelper/DreamKevin"
dreamKevin.fillColor = rgbaToFloat(173, 206, 230, 128)
dreamKevin.borderColor = rgbaToFloat(173, 206, 230, 256)

dreamKevin.associatedMods = {"ChroniaHelper", "CommunalHelper"}

dreamKevin.placements = {
    name = "DreamKevin",
    data = {
        width = 8,
        height = 8,
        axes = "both",
    }
}

dreamKevin.fieldInformation = {
    axes = {
        options = axesOptions,
        editable = false
    }
}

return dreamKevin