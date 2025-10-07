return {
    name = "ChroniaHelper/WaterSurface",
    defaultData = { only ="*", exclude="", flag = "", notflag = "",
        yFar=90, yNear=120, surfaceColor = "ffffff", surfaceColorBack = "ffffff", surfaceAlpha = 0.1, surfaceBackAlpha = 0.1, 
        scrollXFar=1.0, scrollYFar=1.0, scrollXNear=1.2, scrollYNear=1.2, particleColors="ffffff", particleCount = 50, 
        alphaFar = 0.5, alphaNear = 1.0,
        waterSpeedFar=10, waterSpeedNear=20, particleScaleFar=2.0, particleScaleNear=6.0,
        extendedBorderX = 0, extendedBorderY = 0,
        farLineColor = "ffffff", farLineAlpha = 1,
        nearLineColor = "ffffff", nearLineAlpha = 1,
        hasFarLine = false, hasCloseLine = true,
    },
    fieldOrder = {
        "only", "exclude", "flag", "notflag","tag","scrollx","scrolly","Colors"
    },
    canBackground = true,
    fieldInformation = {
        particleColors = {
            fieldType = "list",
            elementOptions = {
                fieldType = "color",
            },
        },
        surfaceColor = {
            fieldType = "color",
        },
        surfaceColorBack = {
            fieldType = "color",
        },
        farLineColor = {
            fieldType = "color",
        },
        nearLineColor = {
            fieldType = "color",
        },
    },
}