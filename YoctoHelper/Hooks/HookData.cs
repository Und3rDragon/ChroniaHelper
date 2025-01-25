namespace YoctoHelper.Hooks;

public class HookData
{

    public object sessionValue { get; set; }

    public object roomValue { get; set; }

    public HookData()
    {
    }

    public HookData(object sessionValue)
    {
        this.sessionValue = sessionValue;
    }

    public HookData(object sessionValue, object roomValue)
    {
        this.sessionValue = sessionValue;
        this.roomValue = roomValue;
    }

    public void SetValue(object value)
    {
        this.sessionValue = value;
        this.roomValue = value;
    }

}
