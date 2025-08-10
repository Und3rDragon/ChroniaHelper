using System.Collections;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using YoctoHelper.Cores;
using ChroniaHelper.Utils;

namespace YoctoHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/TextboxTrigger")]
public class TextboxTrigger : BaseTrigger
{

    private EntityID id;

    private TextboxModes textboxMode;

    private DialogTextModes dialogTextMode;

    private string dialogText;

    private bool endLevel;

    private Coroutine textboxRoutine;

    public TextboxTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
    {
        this.id = id;
        this.textboxMode = data.Enum<TextboxModes>("textboxMode", TextboxModes.Textbox);
        this.dialogTextMode = data.Enum<DialogTextModes>("dialogTextMode", DialogTextModes.DialogId);
        this.dialogText = data.Attr("dialogText", null);
        this.endLevel = data.Bool("endLevel", false);
    }

    protected override void OnEnterHandle(Player player)
    {
        if (ObjectUtils.IsNull(player))
        {
            return;
        }
        if (this.textboxMode == TextboxModes.Textbox)
        {
            this.Textbox(player);
        }
        else if (this.textboxMode == TextboxModes.MiniTextbox)
        {
            this.MiniTextbox();
        }
        if (base.onlyOnce)
        {
            this.level.Session.DoNotLoad.Add(this.id);
        }
    }

    private void Textbox(Player player)
    {
        if (ObjectUtils.IsNull(player))
        {
            return;
        }
        if (this.endLevel)
        {
            base.level.RegisterAreaComplete();
        }
        player.StateMachine.State = Player.StDummy;
        base.level.StartCutscene(this.TextboxEnd);
        base.Add(this.textboxRoutine = new Coroutine(this.TextboxRoutine(), true));
    }

    private IEnumerator TextboxRoutine()
    {
        Textbox textbox = new Textbox(this.dialogText);
        if (this.dialogTextMode == DialogTextModes.Text)
        {
            DynData<Textbox> dynData = new DynData<Textbox>(textbox);
            dynData.Set<FancyText.Text>("text", FancyText.Parse(this.dialogText, (int)dynData.Get<float>("maxLineWidth"), dynData.Get<int>("linesPerPage"), 0F, null, null));
        }
        base.Scene.Add(textbox);
        while (textbox.Opened)
        {
            yield return null;
        }
        base.level.EndCutscene();
        this.TextboxEnd(base.level);
        yield break;
    }

    private void TextboxEnd(Level level)
    {
        Player player = base.Scene.Tracker.GetEntity<Player>();
        if (ObjectUtils.IsNotNull(player))
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }
        this.textboxRoutine.Cancel();
        this.textboxRoutine.RemoveSelf();
        if (this.endLevel)
        {
            base.level.CompleteArea(spotlightWipe: true, skipScreenWipe: false, skipCompleteScreen: false);
            player.StateMachine.State = Player.StDummy;
        }
    }

    private void MiniTextbox()
    {
        MiniTextbox miniTextbox = new MiniTextbox(this.dialogText);
        if (this.dialogTextMode == DialogTextModes.Text)
        {
            DynData<MiniTextbox> dynData = new DynData<MiniTextbox>(miniTextbox);
            dynData.Set<FancyText.Text>("text", FancyText.Parse(this.dialogText, (int)(1688F - dynData.Get<float>("portraitSize") - 32F), 2, 1F, null, null));
        }
        base.Scene.Add(miniTextbox);
    }

}
