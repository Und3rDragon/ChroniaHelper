local outlineCircle = {}

outlineCircle.name = "ChroniaHelper/OutlinerCircle"
outlineCircle.nodeLimits = {1,1}
outlineCircle.placements =
{
    type = "line",
    name = "circle",
    data =
    {
        borderColor = "ffffff",
        borderAlpha = 1,
        innerColor = "ffffff",
        innerAlpha = 0.3,
        borderStyle = 1,
        innerStyle = 1,
        attached = false,
    }
}

outlineCircle.fieldInformation =
{
    borderStyle = {
        fieldType = "integer",
        editable = false,
        options = {
            ["Style 1"] = 1,
            ["Style 2"] = 2,
        },
    },
    innerStyle = {
        fieldType = "integer",
        editable = false,
        options = {
            ["Style 1"] = 1,
            ["Style 2"] = 2,
        },
    },
    borderColor =
    {
        fieldType = "color",
    },
    borderAlpha =
    {
        fieldType = "number",
        allowEmpty = false,
        minimumValue = 0
    },
    innerColor =
    {
        fieldType = "color",
    },
    innerAlpha =
    {
        fieldType = "number",
        allowEmpty = false,
        minimumValue = 0
    }
}

outlineCircle.fieldOrder =
{

}

outlineCircle.ignoredFields = {

}

outlineCircle.sprite = function(room, entity, viewport)
    local path = "ChroniaHelper/Outliner/knot"
    local borderColor = "#ffffff"
    local innerColor = "#ffffff"

    if entity.borderColor ~= "" then
        borderColor = entity.borderColor
    end

    if entity.innerColor ~= "" then
        innerColor = entity.innerColor
    end

    local renderBorderAlpha = entity.borderAlpha
    local renderInnerAlpha = entity.innerAlpha

    if renderBorderAlpha <= 0.1 then
        renderBorderAlpha = 0.2
    end
    if renderInnerAlpha <= 0.1 then
        renderInnerAlpha = 0.1
    end
    borderColor = borderColor .. string.format("%x",(255 * renderBorderAlpha))
    innerColor = innerColor .. string.format("%x",(255 * renderInnerAlpha))
        
    local sprites = {}
        
    local sprite = require("structs.drawable_sprite").fromTexture(path, entity)
        
    local lines = drawCircle({entity.x, entity.y, entity.nodes[1].x, entity.nodes[1].y}, 0.8)
        
    table.insert(sprites, sprite)
        
    for _, line in ipairs(lines) do
        table.insert(sprites, line)
    end
        
    return sprites
end

outlineCircle.depth = function(room,entity) return entity.depth end

return outlineCircle