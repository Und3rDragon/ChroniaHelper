local arbitrary = require("mods").requireFromPlugin("libraries.arbitraryShapeEntity", "FrostHelper")

local killbox = {}

killbox.name = "ChroniaHelper/PolygonKillbox"
killbox.nodeLimits = {2, 999}
killbox.nodeVisibility = "always"
killbox.associatedMods = {"ChroniaHelper", "VivHelper", "FrostHelper"}
killbox.placements =
{
	name = "PolygonKillbox",
	data =
	{
		flag = "",
	}
}
killbox.fieldInformation = {
	
}
killbox.triggerText = function(room, entity)
	local base = "Polygon Killbox"
	return base
end

killbox.sprite = arbitrary.getSpriteFunc("00ff00", "fcf579", "000000", "ff0000")
killbox.nodeSprite = arbitrary.nodeSprite
killbox.selection = arbitrary.selection

return killbox