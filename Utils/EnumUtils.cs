using System;

namespace ChroniaHelper.Utils;

public static class EnumUtils
{

    public static E StringToEnum<E>(string value, E defaultValue) where E : struct
    {
        return Enum.TryParse(value, out E result) ? result : defaultValue;
    }

}
