return {
    triggerText = function(room, entity)
        return "Target Id\n(" .. entity.targetId .. ")"
    end,
    name = "ChroniaHelper/TargetIdTrigger",
    placements =
    {
        name = "TargetIdTrigger",
        data =
        {
            ifFlag = "",
            targetId = ""
        }
    },
    fieldOrder =
    {
        "x",
        "y",
        "width",
        "height",
        "ifFlag",
        "targetId"
    }
}