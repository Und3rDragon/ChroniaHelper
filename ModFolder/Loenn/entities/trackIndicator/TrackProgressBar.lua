local utils = require('utils')
local vivUtilsMig = require('mods').requireFromPlugin('libraries.vivUtilsMig')
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local depthOptions = require("mods").requireFromPlugin("consts.depthOptions")

local bar = {}

bar.name = "ChroniaHelper/TrackProgressBar"
bar.placements = {
	name = "ProgressBar",
	data = {

	},
}