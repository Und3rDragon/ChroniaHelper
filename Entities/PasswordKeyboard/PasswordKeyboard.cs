using Celeste.Mod.Entities;

namespace ChroniaHelper.Entities.PasswordKeyboard;

[CustomEntity("PasswordKeyboard/PasswordKeyboard")]
public sealed partial class PasswordKeyboard : Entity
{
    public enum Mode { Exclusive, Normal, OutputFlag }

    private readonly Config config;
    private readonly EntityID entityID;
    private readonly TalkComponent talkComponent;
    private readonly UI ui;
    private Player lastPlayer;

    public PasswordKeyboard(EntityData data, Vector2 offset)
        : this(
              data.Position + offset,
              new Config(
                  (Mode)data.Int("mode"),
                  data.Attr("flagToEnable"),
                  data.Attr("password"),
                  data.Attr("rightDialog", "rightDialog"),
                  data.Attr("wrongDialog", "wrongDialog"),
                  data.Bool("caseSensitive", false),
                  data.Int("useTimes", -1)
                  ),
              new EntityID(data.Level.Name, data.ID)
              )
    {
    }

    public PasswordKeyboard(Vector2 position, Config config, EntityID entityID)
        : base(position)
    {
        this.config = config;
        this.entityID = entityID;
        Add(new Image(GFX.Game["PasswordKeyboard/keyboard"]).JustifyOrigin(0.5f, 1.0f));
        Add(talkComponent = new TalkComponent(new Rectangle(-16, -4, 32, 4), Vector2.UnitY * -16f, OnTalk));
        talkComponent.PlayerMustBeFacing = true;

        ui = new(config, OnExit, OnTry);
        var dic = ChroniaHelperModule.Session.RemainingUses;
        if (!dic.ContainsKey(entityID))
            dic[entityID] = config.UseTimes;
    }

    private void OnTalk(Player player)
    {
        if (ChroniaHelperModule.Session.RemainingUses[entityID] is 0)
        {
            talkComponent.Active = false;
            return;
        }
        player.StateMachine.State = Player.StDummy;
        ui.Clear();
        Scene.Add(ui);
        lastPlayer = player;
    }

    private bool OnTry(string password)
    {
        var dic = ChroniaHelperModule.Session.RemainingUses;
        switch (config.Mode)
        {
        case Mode.Exclusive:
                ChroniaHelperModule.Session.Password = password;
            break;
        case Mode.Normal:
            if (!password.Equals(
                config.Password,
                config.CaseSensitive ?
                StringComparison.InvariantCulture :
                StringComparison.InvariantCultureIgnoreCase
                ))
                SceneAs<Level>().Session.SetFlag(config.FlagToEnable, true);
            return false;
        case Mode.OutputFlag:
            SceneAs<Level>().Session.SetFlag(password, true);
            break;
        default:
            return false;
        }
        if (dic[entityID] > 0)
            dic[entityID] -= 1;
        return true;
    }

    private void OnExit()
    {
        if (lastPlayer is null) return;
        Scene.Remove(ui);
        lastPlayer.StateMachine.State = Player.StNormal;
    }
}