return {
    levelDeathMode =
    {
        data = "Equal",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "Equal",
                "NotEqual",
                "LessThan",
                "LessThanOrEqual",
                "GreaterThan",
                "GreaterThanOrEqual"
            },
            editable = false
        }
    },
    levelDeathCount =
    {
        data = - 1,
        info =
        {
            fieldType = "integer",
            allowEmpty = false,
            minimumValue = - 1
        }
    },
    totalDeathMode =
    {
        data = "Equal",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "Equal",
                "NotEqual",
                "LessThan",
                "LessThanOrEqual",
                "GreaterThan",
                "GreaterThanOrEqual"
            },
            editable = false
        }
    },
    totalDeathCount =
    {
        data = - 1,
        info =
        {
            fieldType = "integer",
            allowEmpty = false,
            minimumValue = - 1
        }
    },
    enterMode =
    {
        data = "Any",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "None",
                "Top",
                "Right",
                "Bottom",
                "Left",
                "TopLeft",
                "TopRight",
                "BottomLeft",
                "BottomRight",
                "Horizontal",
                "Vertical",
                "Coordinate",
                "ForwardSlash",
                "BackSlash",
                "Slash",
                "Any"
            },
            editable = false
        }
    },
    enterDelay =
    {
        data = 0,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    enterIfFlag =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    enterSound =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    leaveMode =
    {
        data = "Any",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "None",
                "Top",
                "Right",
                "Bottom",
                "Left",
                "TopLeft",
                "TopRight",
                "BottomLeft",
                "BottomRight",
                "Horizontal",
                "Vertical",
                "Coordinate",
                "ForwardSlash",
                "BackSlash",
                "Slash",
                "Any"
            },
            editable = false
        }
    },
    leaveDelay =
    {
        data = 0,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    leaveIfFlag =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    leaveSound =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    leaveReset =
    {
        data = false,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    },
    updateIfFlag =
    {
        data = "",
        info =
        {
            fieldType = "string",
            allowEmpty = true
        }
    },
    updateDelay =
    {
        data = 0,
        info =
        {
            fieldType = "number",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    positionPoint =
    {
        data = "TopLeft",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "TopLeft",
                "TopCenter",
                "TopRight",
                "CenterLeft",
                "Center",
                "CenterRight",
                "BottomLeft",
                "BottomCenter",
                "BottomRight"
            },
            editable = false
        }
    },
    positionMode =
    {
        data = "NoEffect",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "NoEffect",
                "LeftToRight",
                "RightToLeft",
                "TopToBottom",
                "BottomToTop",
                "HorizontalCenter",
                "VerticalCenter"
            },
            editable = false
        }
    },
    playerFacing =
    {
        data = "None",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "None",
                "Left",
                "Right"
            },
            editable = false
        }
    }
}