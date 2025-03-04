using Celeste.Mod.Entities;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Entities.PasswordKeyboard;

[CustomEntity("ChroniaHelper/PasswordKeyboard")]
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
                  data.Attr("tag","passwordKeyboard"),
                  data.Attr("flagToEnable"),
                  data.Attr("password"),
                  data.Attr("rightDialog", "rightDialog"),
                  data.Attr("wrongDialog", "wrongDialog"),
                  data.Bool("caseSensitive", false),
                  data.Int("useTimes", -1),
                  new EntityID(data.Level.Name, data.ID),
                  data.Bool("passwordEncrypted", false),
                  data.Bool("showEncryptedPasswordInConsole", false),
                  data.Attr("texture", "ChroniaHelper/PasswordKeyboard/keyboard"),
                  data.Attr("talkIconPosition", "0,-8")
                  ),
              new EntityID(data.Level.Name, data.ID),
              data
              )
    {
    }

    public PasswordKeyboard(Vector2 position, Config config, EntityID entityID, EntityData data)
        : base(position)
    {
        // modified based on Sap's codes
        string accessZone = data.Attr("accessZone");
        string[] hitbox = accessZone.Split(',', StringSplitOptions.TrimEntries);
        int[] hp = { -16, 0, 32, 8 };
        for(int i = 0; i < Calc.Min(hitbox.Length, 4); i++)
        {
            int p = hp[i];
            int.TryParse(hitbox[i], out p);
            hp[i] = p;
        }
        hp[2].MakeAbs(); hp[3].MakeAbs();
        
        Vector2 iconPos = new Vector2(0f, -8f);
        string[] iconPosSetting = config.TalkIconPosition.Split(",", StringSplitOptions.TrimEntries);
        for(int i = 0; i < iconPosSetting.Length; i++)
        {
            if(i == 0) { float.TryParse(iconPosSetting[0], out iconPos.X); }
            if(i == 1) { float.TryParse(iconPosSetting[1], out iconPos.Y); }
        }

        this.config = config;
        this.entityID = entityID;
        Add(new Image(GFX.Game[config.Texture]).JustifyOrigin(0.5f, 0.5f));
        Add(talkComponent = new TalkComponent(new Rectangle(hp[0], hp[1], hp[2], hp[3]), iconPos, OnTalk));
        talkComponent.PlayerMustBeFacing = true;

        ui = new(config, OnExit, OnTry);
        var dic = ChroniaHelperModule.Session.RemainingUses;
        if (!dic.ContainsKey(entityID))
            dic[entityID] = config.UseTimes;

        // Additional
        globalFlag = data.Bool("globalFlag", false);

        if (config.ShowHash)
        {
            Log.Info($"Generated Hash for Keyboard [{config.IDTag}]:");
            Log.Info($"{StringUtils.GetHashString(config.CaseSensitive ? config.Password : config.Password.ToLower(), config.IDTag)}");
        }

        base.Depth = data.Int("depth", 9000);
    }
    private bool globalFlag = false;

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
        bool feedback = true;
        switch (config.Mode)
        {
            case Mode.Exclusive:
                bool exists = ChroniaHelperModule.Session.Passwords.ContainsKey(config.IDTag);
                if (exists) { ChroniaHelperModule.Session.Passwords[config.IDTag] = password; }
                else { ChroniaHelperModule.Session.Passwords.Add(config.IDTag, password); }
                break;
            case Mode.Normal:
                string passIn = config.CaseSensitive ? password : password.ToLower();
                string passOut = config.CaseSensitive ? config.Password : config.Password.ToLower();
                if (config.Encrypted)
                {
                    passIn = StringUtils.GetHashString(passIn, config.IDTag);
                    passOut = config.Password;
                }
                if(passIn == passOut && (dic[entityID] > 0 || dic[entityID] == -1))
                {
                    FlagUtils.SetFlag(config.FlagToEnable, true);
                }
                //return false;
                feedback = false;
                break;
            case Mode.OutputFlag:
                FlagUtils.SetFlag(password, true);
                break;
            default:
                //return false;
                feedback = false;
                break;
        }
        if (dic[entityID] > 0)
            dic[entityID] -= 1;
        return feedback;
    }

    private void OnExit()
    {
        if (lastPlayer is null) return;
        Scene.Remove(ui);
        lastPlayer.StateMachine.State = Player.StNormal;
    }
}