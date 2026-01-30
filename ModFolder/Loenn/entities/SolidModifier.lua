local drawableRect = require('structs.drawable_rectangle')
local drawableText = require('structs.drawable_text')

return {{
    name = "ChroniaHelper/SolidModifier",
    fieldInformation = {
        cornerBoostBlock = {fieldType = "integer", options = {
            {"Normal", 0},
            {"Based On Speed", -1},
            {"1px", 1}, {"2px", 2}, {"3px", 3}, {"4px", 4}, {"5px", 5}, {"6px", 6}, {"7px", 7}, {"8px", 8}, {"9px", 9}, {"10px", 10}, {"11px", 11}, {"12px", 12}, {"13px", 13}, {"14px", 14}, {"15px", 15}, {"16px", 16}, {"17px", 17}, {"18px", 18}, {"19px", 19}, {"20px", 20}, {"21px", 21}, {"22px", 22}, {"23px", 23}, {"24px", 24}, {"25px", 25}
        }, editable = true},
        TriggerOnTouch = {fieldType = "integer", options = {
            ["No Changes"] = 0,
            ["On Side Contact"] = 1,
            ["On Side + Bottom Contact"] = 2
        }, editable = false},
    },
    fieldOrder = {"x","y","width","height","Types","EntitySelect","CornerBoostBlock","RetainWallSpeed","TriggerOnTouch","TriggerOnBufferInput"},
    placements = {
        {name = "main", data = { width = 8, height = 8, 
            Types = "*Solid", EntitySelect = true,
            cornerBoostBlock = 0,  TriggerOnBufferInput = false, TriggerOnTouch = 0, RetainWallSpeed = false
        }},
        {name = "cb", data = { width = 8, height = 8, Types = "*Solid", EntitySelect = true, 
            cornerBoostBlock = -1, RetainWallSpeed = true
        }},
        {name = "touch", data = { width = 8, height = 8, Types = "*Solid", EntitySelect = true, 
            TriggerOnTouch = 0, TriggerOnBufferInput = false
        }}
    },
    sprite = function(room,entity) 
        return {
            drawableRect.fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height,
            {0.4,0.2,0.2,0.3}, {0.4,0.2,0.2,1}),
            drawableText.fromText("Solid Modifier", entity.x,entity.y,entity.width,entity.height,nil,0.5)
        } 
    end,
    depth = -100000,
}}