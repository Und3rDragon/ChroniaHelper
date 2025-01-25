namespace ChroniaHelper.Modules;

public class ChroniaHelperHandle
{

    public void LoadHandle()
    {
        ChroniaHelperModule.Instance.HookManager.Load();
        On.Celeste.Player.ctor += this.Player_ctor;
        Everest.Events.Player.OnSpawn += this.Player_OnSpawn;
        Everest.Events.Level.OnTransitionTo += this.Level_OnTransitionTo;
    }

    public void UnloadHandle()
    {
        ChroniaHelperModule.Instance.HookManager.Unload();
        On.Celeste.Player.ctor -= this.Player_ctor;
        Everest.Events.Player.OnSpawn -= this.Player_OnSpawn;
        Everest.Events.Level.OnTransitionTo -= this.Level_OnTransitionTo;
    }

    private void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
    {
        orig(self, position, spriteMode);
        ChroniaHelperModule.Instance.HookManager.ResetHookDataRoomValue();
    }

    private void Player_OnSpawn(Player obj)
    {
        ChroniaHelperModule.Instance.HookManager.ResetHookDataRoomValue();
    }

    private void Level_OnTransitionTo(Level level, LevelData next, Vector2 direction)
    {
        ChroniaHelperModule.Instance.HookManager.ResetHookDataRoomValue();
    }

}
