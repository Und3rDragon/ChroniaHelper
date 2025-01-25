return {
    name = "ChroniaHelper/CustomRain",
    defaultData = { only ="*", exclude="", flag = "", notflag = "", tag="",
        Scrollx=1.0, Scrolly=1.0, angle=270.0, angleDiff=2.86, speedMult=1.0, Amount=240, Colors="ffffff", alpha=1.0
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
    },
}