using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities.PasswordKeyboard;

[CustomEntity("ChroniaHelper/PasswordKeyboard")]
public sealed partial class PasswordKeyboard : Entity
{
    public enum Mode { Exclusive, Normal, OutputFlag, Systematic }

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
                  data.Bool("globalFlag", false),
                  data.Bool("toggleFlag", false),
                  data.Attr("texture", "ChroniaHelper/PasswordKeyboard/keyboard"),
                  data.Attr("talkIconPosition", "0,-8"),
                  data.Int("characterLimit", 12)
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
        var dic = Md.Session.Passkeyboard_RemainingUses;
        if (!dic.ContainsKey(entityID))
            dic[entityID] = config.UseTimes;

        // Additional
        // Multi-password support
        globalFlag = data.Bool("globalFlag", false);

        passwords = config.Password.Split(";", StringSplitOptions.TrimEntries);
        passwordCount = passwords.Length;
        flagList = config.FlagToEnable.Split(',', StringSplitOptions.TrimEntries);
        flagCount = flagList.Length;
        if (config.ShowHash)
        {
            Log.Info($"Generated Hash for Keyboard [{config.IDTag}]:");
            if (passwordCount == 1)
            {
                Log.Info($"{StringUtils.GetHashString(config.IDTag + config.Password, config.CaseSensitive)}");
            }
            else if (passwordCount > 1)
            {
                for (int i = 0; i < passwordCount; i++)
                {
                    Log.Info($"Password No.{i + 1}:");
                    Log.Info($"{StringUtils.GetHashString(config.IDTag + passwords[i], config.CaseSensitive)}");
                }
            }
        }
        Md.Session.Passkeyboard_PasswordQueue.Enter(entityID, 0);

        base.Depth = data.Int("depth", 9000);
    }
    private bool globalFlag = false;
    private string[] passwords, flagList; 
    private int passwordCount = 0, flagCount = 0;

    private void OnTalk(Player player)
    {
        if (Md.Session.Passkeyboard_RemainingUses[entityID] is 0)
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
        var dic = Md.Session.Passkeyboard_RemainingUses;
        bool feedback = true; // when feedback is true, the keyboard will exit after input
        switch (config.Mode)
        {
            case Mode.Exclusive:
                Md.Session.Passkeyboard_Passwords.Enter(config.IDTag, password);

                feedback = true; break;
            case Mode.Normal:
                int nextPassword = Math.Min(Md.Session.Passkeyboard_PasswordQueue[entityID], passwordCount - 1);

                string passIn = config.CaseSensitive ? password : password.ToLower();
                string passOut = config.CaseSensitive ? passwords[nextPassword] : passwords[nextPassword].ToLower();
                if (config.Encrypted)
                {
                    passIn = StringUtils.GetHashString(config.IDTag + password, config.CaseSensitive);
                    passOut = passwords[nextPassword];
                }

                string currentFlag = flagList[Math.Min(Md.Session.Passkeyboard_PasswordQueue[entityID], flagCount - 1)];
                if(passIn == passOut && (dic[entityID] > 0 || dic[entityID] == -1))
                {
                    if (config.Toggle)
                    {
                        ChroniaFlagUtils.SetFlag(currentFlag, !ChroniaFlagUtils.GetFlag(currentFlag), config.Global);
                    }
                    else
                    {
                        ChroniaFlagUtils.SetFlag(currentFlag, true, config.Global);
                    }

                    Md.Session.Passkeyboard_PasswordQueue[entityID]++;
                    feedback = true; break;
                }
                
                feedback = false; break;
            case Mode.OutputFlag:
                if (config.Toggle)
                {
                    ChroniaFlagUtils.SetFlag(password, !ChroniaFlagUtils.GetFlag(password), config.Global);
                }
                else
                {
                    ChroniaFlagUtils.SetFlag(password, true);
                }

                feedback = true; break;
            case Mode.Systematic:
                passIn = config.CaseSensitive ? password : password.ToLower();
                feedback = false;
                for (int i = 0; i < passwordCount; i++)
                {
                    passOut = config.CaseSensitive ? passwords[i] : passwords[i].ToLower();
                    if (config.Encrypted)
                    {
                        passIn = StringUtils.GetHashString(config.IDTag + password, config.CaseSensitive);
                        passOut = passwords[i];
                    }

                    if (passIn == passOut && (dic[entityID] > 0 || dic[entityID] == -1))
                    {
                        if (config.Toggle)
                        {
                            ChroniaFlagUtils.SetFlag(flagList[Math.Min(i, flagCount)], !ChroniaFlagUtils.GetFlag(flagList[Math.Min(i, flagCount)]), config.Global);
                        }
                        else
                        {
                            ChroniaFlagUtils.SetFlag(flagList[Math.Min(i, flagCount)], true, config.Global);
                        }
                        feedback = true; 
                    }
                }

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