local timerControlTrigger = {}

timerControlTrigger.name = "ChroniaHelper/TimerControlTrigger"

local controlOptions = {
    None = "None",
    Start = "Start",
    Pause = "Pause",
    Unpuase = "Unpause",
    Reset = "Reset",
    Complete = "Complete",
    Set = "Set",
    Add = "Add",
    Subtract = "Subtract"
}

local flagOptions = {
    None = "Noneone",
    Set = "Set",
    Remove = "Remove",
    --SetIfValid = "Set If Valid",
}

local conditionOptions = {
    None = "None",
    Equal = "Equal",
    NotEqual = "NotEqual",
    Less = "Less",
    LessEqual = "LessEqual",
    Greater = "Greater",
    GreaterEqual = "GreaterEqual"
}

timerControlTrigger.placements = {
    {
        name = "TimerControlTrigger",
        data = {
            width = 16,
            height = 16,
            controlType = "None",
            flagType = "None",
            conditionType = "None",
            timeInput = "00:00:00.000",
            recordID = "",
            flag = "",
            recordTime = false,
            onlyOnce = false,
            useRecordAsInput = false
        }
    }
}

timerControlTrigger.fieldInformation = 
{
    controlType = {
        options = controlOptions,
        editable = false
    },
    flagType = {
        options = flagOptions,
        editable = false
    },
    conditionType = {
        options = conditionOptions,
        editable = false
    }
}

return timerControlTrigger