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
		indicatorSprite = "objects/ChroniaHelper/spriteEntity/testB0",
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
				["play: spriteName"] = "play,spriteName",
				["flag_play: flag, spriteName, (inverted)"] = "flag_play,flag,spriteName,false",
				["wait: period"] = "wait,1",
				["wait_flag: flag, (inverted)"] = "wait_flag,flag,false",
				["move: deltaX, deltaY, (moveTime, easing)"] = "move,16,-16,0,linear",
				["move_to: X, Y, (moveTime, easing)"] = "move_to,0,0,0,linear",
				["alpha: setAlpha, (changeTime, easing)"] = "alpha,1,0,linear",
				["color: HEX, (changeTime, easing)"] = "color,ffffff,0,linear",
				["scale: scaleX, scaleY, (changetime, easing)"] = "scale,1,1,0,linear",
				["rotate: angle, (rotateTime, easing, isDelta)"] = "rotate,45,0,linear,false",
				["depth: setDepth, (transferTime, easing)"] = "depth,9500,0,linear",
				["repeat: repeatFromCommandIndex, overrideFlag"] = "repeat,0,repeatOverride",
				["move_around: center dX, center dY, deltaAngle, spinTime, (easing)"] = "move_around,1,0,45,0,linear",
				["origin: originX, originY, (changeTime, easing)"] = "origin,0.5,0.5,0,linear",
				["rate: animationRate, (changeTime, easing)"] = "rate,1,0,linear",
				["ignore: ignoreFlag, flagInverted, num1, (num2...)"] = "ignore,ignoreFlag,false,0",
				["sound: soundFX name"] = "sound,soundName",
				["music: trackName"] = "music,trackName",
				["hitbox: width, height, (x, y)"] = "hitbox,16,16,-8,-8",
				["passby functionName"] = "passby xxx,xxx",
			},
			editable = true,
		},
	},
	depth = {
		options = depthOptions,
		editable = true,
	}
}

se.sprite = function(room, entity)
	return drawableSprite.fromTexture("objects/ChroniaHelper/spriteEntity/testB0", entity)
end


return se