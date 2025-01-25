return{
	category = "camera",
	name = "ChroniaHelper/AxisCameraOffset",
	placements =
	{
		name = "AxisCameraOffset",
		data =
		{
			cameraX = 0,
			cameraY = 0,
			cameraMode = "normal",
			triggerMode = "toggle",
			flagControl = "disabled",
			flag = "flag",
			units = "offset",
			onlyOnce = false,
		}
	},
	fieldInformation = {
		cameraMode = {
			options = {
				"normal",
				"X Only",
				"Y Only",
			},
			editable = false,
		},
		triggerMode = {
			options = {
				"toggle",
				"inZone",
			},
			editable = false,
		},
		flagControl = {
			options = {
				"disabled",
				"flagNeeded",
				"flagInverted",
			},
			editable = false,
		},
		units = {
			options = {
				"offset",
				"offsetSquared",
				"tiles",
				"pixels",
			},
			editable = false,
		},
	},
	triggerText = function(room, entity)
		local base ="Camera Offset"
		local cam = "(" .. entity.cameraX .. "," .. entity.cameraY .. ")"
		if entity.cameraMode == "X Only" then
			cam = "(X=" .. entity.cameraX .. ")"
		elseif entity.cameraMode == "Y Only" then
			cam = "(Y=" .. entity.cameraY .. ")"
		end
		local suffix = ""
		if entity.triggerMode == "inZone" then
			suffix = suffix .. " (inzone)"
		end
		if entity.flagControl == "flagNeeded" then
			suffix = suffix .. " (flag=" .. entity.flag .. ")"
		elseif entity.flagControl == "flagInverted" then
			suffix = suffix .. " (flag=!" .. entity.flag .. ")"
		end
		return base .. " " .. cam .. suffix
	end
}