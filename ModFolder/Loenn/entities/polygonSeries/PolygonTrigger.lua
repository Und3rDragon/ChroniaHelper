local arbitrary = require("mods").requireFromPlugin("libraries.arbitraryShapeEntity", "FrostHelper")

local trigger = {}

trigger.name = "ChroniaHelper/PolygonTrigger"
trigger.nodeLimits = {2, 999}
trigger.nodeVisibility = "always"
trigger.associatedMods = {"ChroniaHelper", "VivHelper", "FrostHelper"}
trigger.placements = {
	name = "PolygonTrigger",
	data =
	{
		types = "",
		oneUse = false,
	}
}
trigger.fieldInformation = {

}
trigger.triggerText = function(room, entity)
	local base = "Polygon Trigger"

	return base
end

trigger.sprite = arbitrary.getSpriteFunc("00ff00", "fcf579", "000000", "ff0000")
trigger.nodeSprite = arbitrary.nodeSprite
trigger.selection = arbitrary.selection

return trigger