

local ccm = {name = "ChroniaHelper/CustomCoreMessage"}

ccm.texture = "@Internal@/core_message"
ccm.depth = -10000000

ccm.placements = {
    name = "CustomCoreMessage",
    data = {
        line = 0,
        dialog = "app_ending",
        OutlineColor="000000",
        Scale=1.25,
        RenderDistance=128.0,
        AlwaysRender=false,
        LockPosition=false,
        DefaultFadedValue=0.0,
        PauseType="Hidden",
        TextColor1="ffffff",
        EaseType="CubeInOut",
        wholeDialog = false,
        parallax = 1.2,
        screenPosX = 160,
        screenPosY = 90,
    }, nodeLimits = {0,2}
}

ccm.fieldInformation = {
    dialog = {
        options = {
            "ChroniaHelperTimer",
            "ChroniaHelperTimerStatic",
            "ChroniaHelperFrames",
            "ChroniaHelperFramesStatic",
        },
        editable = true,
    },
    OutlineColor = {fieldType = "color", allowXNAColors=true, allowEmpty = true},
    TextColor1 = {fieldType = "color", allowXNAColors=true},
    Scale = {fieldType = "number", minimumValue = 0.125},
    EaseType = {
        options = {
            "Linear",
            "SineIn",
            "SineOut",
            "SineInOut",
            "QuadIn",
            "QuadOut",
            "QuadInOut",
            "CubeIn",
            "CubeOut",
            "CubeInOut",
            "QuintIn",
            "QuintOut",
            "QuintInOut",
            "ExpoIn",
            "ExpoOut",
            "ExpoInOut",
            "BackIn",
            "BackOut",
            "BackInOut",
            "BigBackIn",
            "BigBackOut",
            "BigBackInOut",
            "ElasticIn",
            "ElasticOut",
            "ElasticInOut",
            "BounceIn",
            "BounceOut",
            "BounceInOut"
        },
        editable = false,
    },
    line = { fieldType = "integer", minimumValue = 0 },
    PauseType = {fieldType = "string", options = {"Hidden","Shown","Fade"}, editable = false}
}

return ccm