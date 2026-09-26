local drawableText = require("structs.drawable_text")

return{
	name = "ChroniaHelper/MoveBlockOutlineRemover",
	placements =
	{
		name = "outlineRemover",
		data =
		{
			
		}
	},
	fieldInformation = {
		
	},
	triggerText = function(room, entity)
		return "Outline Remover"
	end
}