return{
	category = "camera",
	name = "ChroniaHelper/SpeedAdaptiveCamera",
	placements =
	{
		name = "Speed Adaptive Camera",
		data =
		{
			multiplierX = 1,
			multiplierY = 1,
			cameraMode = "normal",
			triggerMode = "toggle",
			flagControl = "disabled",
			flag = "flag",
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
	}
}