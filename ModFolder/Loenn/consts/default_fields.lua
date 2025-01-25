return {
    boolMode =
    {
        data = "None",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "None",
                "True",
                "False"
            },
            editable = false
        }
    },
    width =
    {
        data = 8,
        info =
        {
            fieldType = "integer",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    height =
    {
        data = 8,
        info =
        {
            fieldType = "integer",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    freeze =
    {
        data = 0,
        info =
        {
            fieldType = "integer",
            allowEmpty = false,
            minimumValue = 0
        }
    },
    onlyOnce =
    {
        data = false,
        info =
        {
            fieldType = "boolean",
            allowEmpty = false
        }
    },
    direction =
    {
        data = "Up",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "Up",
                "Down",
                "Left",
                "Right"
            },
            editable = false
        }
    },
    ease =
    {
        data = "None",
        info =
        {
            fieldType = "string",
            allowEmpty = false,
            options =
            {
                "None",
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
            editable = false
        }
    }
}