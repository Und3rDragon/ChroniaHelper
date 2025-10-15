return {
    name = "ChroniaHelper/WindRainFG",
    defaultData = { only ="*", exclude="", flag = "", notflag = "",
        scrollx=1.0, scrolly=1.0, windStrength=1.0, Colors="ffffff", Amount = 240, alpha = 1.0,
        extendedBorderX = 0, extendedBorderY = 0, fadingX = "", fadingY = "",
    },
    fieldOrder = {
        "only", "exclude", "flag", "notflag","tag","scrollx","scrolly","windStrength","Colors"
    },
    canBackground = true,
    fieldInformation = {
        Colors = {
            fieldType = "list",
            elementOptions = {
                fieldType = "color",
            },
        },
        fadingX = {
            fieldType = "list",
            elementOptions = {
                fieldType = "list",
                minimumElements = 4,
                maximumElements = 4,
            },
            elementSeparator = ";",
        },
        fadingY = {
            fieldType = "list",
            elementOptions = {
                fieldType = "list",
                minimumElements = 4,
                maximumElements = 4,
            },
            elementSeparator = ";",
        },
    },
}