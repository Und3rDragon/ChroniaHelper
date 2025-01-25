local script = {}

script.name = "spinnerReplace2"
script.displayName = "Replace Viv Spinners to Chronia Spinners 2"

script.tooltip = "Converts VivHelper Spinners to Chronia Helper Spinners (For even older versions)"

--[[
script.parameters = {
    foreDirectory = "blue",
    backDirectory = "blue",
    attachToSolid = false,
    hitboxType = "default",
    dust = false,
    customHitbox = "r,16,16,-8,-8;c,8,-8,-8",
    depth = -8500,
    solidTileCutoff = true,
    bloomAlpha = 0.0,
    bloomRadius = 0.0,
}
]]



--script.fieldOrder = {}

function script.run(room, args)
    for _, entity in ipairs(room.entities) do
        if entity._name == "VivHelper/CustomSpinner" then
            entity._name = "ChroniaHelper/SeamlessSpinner"
            entity.foreDirectory = entity.Directory .. "/fg_" .. entity.Subdirectory
            entity.backDirectory = string.gsub(entity.Directory .. "/fg_" .. entity.Subdirectory, "fg_", "bg_")
            entity.attachToSolid = entity.AttachToSolid
            entity.hitboxType = "default"
            entity.dust = false
            entity.customHitbox = "r,16,16,-8,-8;c,8,-8,-8"
            entity.depth = entity.Depth
            entity.solidTileCutoff = true
            entity.bloomAlpha = 0
            entity.bloomRadius = 0
            entity.noBorder = false

            --print(require("utils").serialize(entity))
        end
    end
end

return script