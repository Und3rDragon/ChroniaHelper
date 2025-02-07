local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('libraries.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depthOptions")

local se = {}

se.depth = function(room,entity) return entity.depth or 9500 end

se.name = "ChroniaHelper/SpriteEntity"
se.placements = {
	name = "SpriteEntity",
	data = {
		depth = 9500,
		parallax = 1.0,
		camPositionX = 160,
		camPositionY = 90,
		indicatorSprite = "objects/ChroniaHelper/spriteEntity/indicator",
		indicatorColor = "ffffff",
		xmlLabel = "SpriteEntity",
		commands = "",
	}
}

se.fieldInformation = {
	commands = {
		fieldType = "list",
		elementSeparator = ";",
		elementOptions = {
			options = {
				["set_flag: flagName, flagState"] = "set_flag,flag,true",
				["play: spriteName, (shouldReset, randomFrame)"] = "play,spriteName",
				["flag_play: flag, spriteName, (inverted, shouldReset, randomFrame)"] = "flag_play,flag,spriteName,false",
				["wait: period"] = "wait,1",
				["wait_flag: flag, (inverted)"] = "wait_flag,flag,false",
				["move: deltaX, deltaY, (moveTime, easing)"] = "move,16,-16,0,linear",
				["light_move: deltaX, deltaY, (moveTime, easing)"] = "light_move,16,-16,0,linear",
				["bloom_move: deltaX, deltaY, (moveTime, easing)"] = "bloom_move,16,-16,0,linear",
				["move_to: X, Y, (moveTime, easing)"] = "move_to,0,0,0,linear",
				["light_moveto: X, Y, (moveTime, easing)"] = "light_moveto,0,0,0,linear",
				["bloom_moveto: X, Y, (moveTime, easing)"] = "bloom_moveto,0,0,0,linear",
				["move_around: center dX, center dY, deltaAngle, spinTime, (easing)"] = "move_around,1,0,45,0,linear",
				["light_movearound: center dX, center dY, deltaAngle, spinTime, (easing)"] = "light_movearound,1,0,45,0,linear",
				["bloom_movearound: center dX, center dY, deltaAngle, spinTime, (easing)"] = "bloom_movearound,1,0,45,0,linear",
				["alpha: setAlpha, (changeTime, easing)"] = "alpha,1,0,linear",
				["color: HEX, (changeTime, easing)"] = "color,ffffff,0,linear",
				["scale: scaleX, scaleY, (changetime, easing)"] = "scale,1,1,0,linear",
				["rotate: angle, (rotateTime, easing, isDelta)"] = "rotate,45,0,linear,false",
				["depth: setDepth, (transferTime, easing)"] = "depth,9500,0,linear",
				["repeat: repeatFromCommandIndex, overrideFlag"] = "repeat,0,repeatOverride",
				["origin: originX, originY, (changeTime, easing)"] = "origin,0.5,0.5,0,linear",
				["rate: animationRate, (changeTime, easing)"] = "rate,1,0,linear",
				["ignore: ignoreFlag, flagInverted, num1, (num2...)"] = "ignore,ignoreFlag,false,0",
				["sound: soundFX name"] = "sound,soundName",
				["music: trackName"] = "music,trackName",
				["hitbox: width, height, (x, y)"] = "hitbox,16,16,-8,-8",
				["light: color, alpha, startFade, endFade, (changeTime, easing)"] = "light,ffffff,0,32,64,0,linear",
				["bloom: alpha, radius, (changeTime, easing)"] = "bloom,0,0,0,linear",
				["parallax: value, (changeTime, easing)"] = "parallax,1",
				["render_position_inroom: posX, posY, (changeTime, easing)"] = "render_position_inroom,160,90",
				["camera_offset: offsetX, offsetY, (changeTime, easing)"] = "camera_offset,0,0",
				["solid: offsetX, offsetY, width, height, (safe)"] = "solid,0,0,0,0",
				["passby functionName"] = "passby xxx,xxx",
			},
			editable = true,
		},
	},
	depth = {
		options = depthOptions,
		editable = true,
	},
	indicatorSprite = vivUtilsMig.GetFilePathWithNoTrailingNumbers(false),
	indicatorColor = {
		fieldType = "color",
	},
}

se.sprite = function(room, entity)
	return vivUtilsMig.getImageWithNumbers(entity.indicatorSprite, 0, entity)
end


return se