namespace ChroniaHelper.Entities.PasswordKeyboard;

partial class PasswordKeyboard
{
    public sealed record Config(
        Mode Mode,
        string IDTag,
        string FlagToEnable,
        string Password,
        string RightDialog,
        string WrongDialog,
        bool CaseSensitive,
        int UseTimes,
        EntityID entityID
        );
}