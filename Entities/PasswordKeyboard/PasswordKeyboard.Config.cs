namespace ChroniaHelper.Entities.PasswordKeyboard;

partial class PasswordKeyboard
{
    public sealed record Config(
        Mode Mode,
        string FlagToEnable,
        string Password,
        string RightDialog,
        string WrongDialog,
        bool CaseSensitive,
        int UseTimes
        );
}