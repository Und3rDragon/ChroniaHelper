return{
	name = "ChroniaHelper/AutoDialogSkipTrigger",
	placements =
	{
		name = "AutoSkipTrigger",
		data =
		{
			deathCount = -1,
			dialogId = "",
			onlyOnce = true,
			endLevel = false,
		}
	},
	triggerText = function(room, entity)
		local base = "Auto Skip (" .. entity.dialogId .. ")"

		if entity.endLevel then
			base = base .. " (EndLevel)"
		end
		return base
	end
}