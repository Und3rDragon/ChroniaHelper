return {
    name = "ChroniaHelper/CustomRain",
    defaultData = { only ="*", exclude="", flag = "", notflag = "", tag="",
        Scrollx=1.0, Scrolly=1.0, angle=270.0, angleDiff=2.86, speedMult=1.0, Amount=240, Colors="161933", alpha=1.0,
        windStrength = 0,
        extendedBorderX = 0, extendedBorderY = 0, fadingX = "", fadingY = "",
    },
    fieldOrder = {
        "only", "exclude", "flag", "notflag", "Scrollx","Scrolly","speedMult","Amount","Colors","alpha"
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