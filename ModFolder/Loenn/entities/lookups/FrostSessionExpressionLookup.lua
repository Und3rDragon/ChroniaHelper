local picker = {}

picker.name = "ChroniaHelper/SessionExpressionLookup"

picker.placements = {
    name = "Frost Helper Session Expression Lookup",
    data = {
        basics = "",
        commands = "",
        properties = "",
        functions = "",
        inputs = "",
    },
}

picker.fieldInformation = function(entity)
    local orig = {}
        
    orig["basics"] = {
        options = {
            "[flag] name",
            "[invert flag] !name",
            "[counter] #name",
            "[slider] @name",
            "[command] $cmd",
            "[function] $func(arg1, arg2,...)",
            "[logic] a && b || c",
            "[arithmetic] + - * / % // < > == <= >= !=",
            "[string] $(content)",
        },
        editable = true,
    }

    orig["commands"] = {
        options = {
            "$allowDistort => bool, Everest-extended Photosensitive Settings",
            "$allowGlitch => bool, Everest-extended Photosensitive Settings",
            "$allowLightning => bool, Everest-extended Photosensitive Settings",
            "$allowScreenFlash => bool, Everest-extended Photosensitive Settings",
            "$allowTextHighlight => bool, Everest-extended Photosensitive Settings",
            "$coreMode => int, 0 is none, 1 is hot, 2 is cold",
            "$dashes => int",
            "$deaths => int",
            "$deathsHere => int",
            "$dtime => float",
            "$e => float, constant",
            "$flags => string collection",
            "$hasGolden => bool",
            "$maxDashes => int",
            "$photosensitive => bool",
            "$pi => float, constant",
            "$player => entity",
            "$restartedFromGolden => bool",
            "$roomName => string",
            "$speed => vector2",
            "$stamina => float",
            "$strawberries => int",
            "$time => float, Scene.TimeActive",
        },
        editable = true,
    }

    orig["properties"] = {
        options = {
            "[parse string] .str(format) or .str()",
            "[string props] .len .isMatch(regex)",
            "[color props] .r .g .b .a",
            "[entity props] .x .y .pos .id",
            "[entityID props] .roomName .id",
            "[collection props] .count .sum(lambda callback) .all(predictae) .any(predicate)",
            "[lambda expression] $name => expression that may contains $name",
            "[player] $player",
            "[player props] .followers",
            "[vector2 props] .x .y .len .lenSq",
        },
        editable = true,
    }

    orig["functions"] = {
        options = {
            "$abs(float x) -> float - Returns the absolute value of x.",
            "$cbrt(float x) -> float - Square root of x.",
            "$clamp(float x, float minVal, float maxVal) -> float - Clamps the value of x so that its between min and max.",
            "$cos(float x) -> float - Calculates trigonometrical functions, x is assumed to be in radians. (Tip: $pi)",
            "$dialog(string dialogId) -> string - Gets the dialog text in the current language for the given dialogId.",
            "$exp(float x) -> float - Cube root of x.",
            "$exp2(float x) -> float - $e raised to the power of x",
            "$flags(string regex) -> IEnumerable<string> - Creates a IEnumerable<string> containing all currently set flags matching the given regex.",
            "$hsv(float h, float s, float v) -> Color - Creates a Color using h, s, v values, assumed to be in range 0-1.",
            "$lerp(float x, float y, float amount) -> float - Performs a linear interpolation between two values based on the given weight. Params: x — The first value, which is intended to be the lower bound. y — The second value, which is intended to be the upper bound. amount — A value between 0 and 1, that indicates the weight of the interpolation.",
            "$log(float x, float y) -> float - The base-y logarithm of x.",
            "$log10(float x) -> float - The base-10 logarithm of x.",
            "$log2(float x) -> float - The base-2 logarithm of x.",
            "$logn(float x) -> float - The natural logarithm of x.",
            "$max(float ...) -> float - Returns the largest value from all provided arguments.",
            "$min(float ...) -> float - Returns the smallest value from all provided arguments.",
            "$pow(float x, float y) -> float - x raised to the power of y.",
            "$pow2(float x) -> float - x raised to the power of 2.",
            "$range(int min, int max) -> IEnumerable<int> - Creates a IEnumerable<int> containing numbers between min (inclusive) and max (exclusive).",
            "$rgb(int r, int g, int b) -> Color - Creates a Color using r, g, b values, assumed to be in range 0-255.",
            "$round(float x) -> float - Rounds the value.",
            "$sin(float x) -> float - Calculates trigonometrical functions, x is assumed to be in radians. (Tip: $pi)",
            "$sqrt(float x) -> float - x raised to the power of 2.",
            "$tan(float x) -> float - Calculates trigonometrical functions, x is assumed to be in radians. (Tip: $pi)",
            "$truncate(float x) -> float - Truncates the value.",
            "$vec(float x, float y) -> Vector2 - Creates a Vector2 with the given x, y values.",
            "$yoyo(float x) -> float - x <= 0.5 ? x * 2 : 1.0 - (value - 0.5) * 2.0).",    
        },
        editable = true,    
    }

    orig["inputs"] = {
        fieldType = "list",
        elementSeparator = "",
        elementOptions = {
            options ={
                "$input.esc", 
                "$input.pause",
                "$input.menuLeft", 
                "$input.menuRight", 
                "$input.menuUp", 
                "$input.menuDown",
                "$input.menuConfirm", 
                "$input.menucancel", 
                "$input.menujournal",
                "$input.quickrestart",
                "$input.jump",
                "$input.dash",
                "$input.grab",
                "$input.talk",
                "$input.crouchDash", 
                "$input.mod.modName.buttonName",
                ".check",
                ".pressed",
                ".released",
                "$input.aim => vector2",
                "$input.feather => vector2",
                "$input.mountainaim => vector2",
                ".x",
                ".y",
            },
            editable = true, 
        },   
    }
    
    return orig
end

function picker.sprite(room, entity)
    local sprite = {}
    
    local iconSprite = require("structs.drawable_sprite").fromTexture("ChroniaHelper/LoennIcons/Folder", entity)
    
    table.insert(sprite, iconSprite)
    
    return sprite
end

return picker