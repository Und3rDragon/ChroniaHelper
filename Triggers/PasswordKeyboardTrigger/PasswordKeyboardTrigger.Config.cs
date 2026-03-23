namespace ChroniaHelper.Entities.PasswordKeyboard;

partial class PasswordKeyboardTrigger
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
        EntityID entityID,
        bool Encrypted,
        bool ShowHash,
        bool Global,
        bool Toggle,
        string Texture,
        string TalkIconPosition,
        int LengthLimit
        );
}