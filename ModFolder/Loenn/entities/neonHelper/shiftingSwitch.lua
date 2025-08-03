-- The source code of this entity is migrated from NeonHelper, which is integrated in City of Broken Dreams
-- The original author is ricky06, code modified by UnderDragon

local shiftingSwitch = {}

shiftingSwitch.name = "ChroniaHelper/ShiftingSwitch"
shiftingSwitch.depth = 0
shiftingSwitch.texture = "objects/NeonCity/shiftingSwitch/idle00"
shiftingSwitch.nodeTexture = nil
shiftingSwitch.nodeLimits = {0, -1}
shiftingSwitch.nodeLineRenderType = "fan"

shiftingSwitch.placements = {
    name = "Shifting Switch",
    data = {
        left = true,
        right = true,
        top = true,
        down = true,
        speed = 3.0,
        distance = 32.0,
    }
}

shiftingSwitch.justification = {0, 0}

return shiftingSwitch