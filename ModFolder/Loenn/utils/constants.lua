local cons = {}

--[[
local cons = require("mods").requireFromPlugin("utils.constants")
]]

cons.depths = {
	fieldType = "integer",
	options = {
		10000,
		9500,
		9000,
		8000,
		5000,
		2000,
		1000,
		100,
		0,
		-50,
		-100,
		-200,
		-8000,
		-8500,
		-9000,
		-10000,
		-10500,
		-11000,
		-12000,
		-12500,
		-13000,
		-50000,
		-1000000,
		-2000000,
	},
	editable = true,
}

return cons