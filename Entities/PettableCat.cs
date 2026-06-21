using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/PettableCat")]
[Credits("HollyMagala for HonlyHelper source code" +
         "zXnder for default fox sprite")]
public class PettableCat : NPC
{

    private Coroutine pettingRoutine;
    private Sprite CatSprite;
    private Sprite ThePetterSprite;
    private bool PettingInProgress = false;
    private PlayerSprite backup;
    private string catFlag;
    private Vector2 friendposition;

    public int petGroup;
    private string catPetSound;

    private Vc2 interactor;

    public PettableCat(EntityData data, Vector2 offset)
       : base(data.Position + offset)
    {
        CatSprite = GFX.SpriteBank.Create(data.Attr(
            "catSpriteXML", "ChroniaHelper_Cat"));
        CatSprite.Justify = new Vc2(0.5f, 1f);
        Add(CatSprite);

        ThePetterSprite = GFX.SpriteBank.Create(
            data.Attr("petterSpriteXML", "HonlyHelper_CatPetter"));
        // check sprite path
        //if(ThePetterSprite.animations["idle"].Frames.Count() == 1)
        //{
        //    if(ThePetterSprite.animations["idle"].Frames[0].AtlasPath == "__fallback")
        //    {
        //        //Use default setup
        //        ThePetterSprite.animations["idle"] = GFX.SpriteBank.Create("player").animations["idle"];
        //    }
        //}
        ThePetterSprite.Justify = new Vc2(0.5f, 1f);
        Add(ThePetterSprite);
        ThePetterSprite.Position = CatSprite.Position + new Vector2(-8f, 0f);

        catFlag = data.Attr("catFlag");

        // New customizations
        catPetSound = data.Attr("catPetSound", "event:/HonlyHelper/catsfx");

        Depth = data.Int("depth", 0);

        interactor = new Vc2(0f, data.Float("interactorY", -4f));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Add(Talker = new TalkComponent(new Rectangle(-32, -8, 64, 8), interactor, OnPetting));
        //CatAnchor = CatSprite.Position;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        CatSprite.Play("idle");
        //ThePetterSprite.Play("idle");
        ThePetterSprite.Visible = false;
    }

    private void OnPetting(Player player)
    {
        Level.StartCutscene(OnPettingEnd);
        Add(pettingRoutine = new Coroutine(ThePetting(player)));
    }

    private IEnumerator ThePetting(Player player)
    {
        yield return PlayerApproachLeftSide(player, turnToFace: true, 6f); //Level.ZoomBack(0.5f);
        friendposition = player.Sprite.Position;
        PettingInProgress = true;
        CatSprite.Play("pet");
        //CatSprite.Position = CatAnchor + new Vector2(-4f, -8f);
        ThePetterSprite.Visible = true;
        ThePetterSprite.Play("pet");
        backup = player.Sprite;
        player.Sprite.Position = friendposition + Vector2.UnitY * 1000f;
        Audio.Play(catPetSound, base.Center);

        //Remove(Talker);
        yield return 2f; //Level.ZoomBack(0.5f);
        Level.EndCutscene();
        OnPettingEnd(Level);
        CatSprite.Play("idle");
        //CatSprite.Position = CatAnchor;
        base.SceneAs<Level>().Session.SetFlag(catFlag);
    }

    private void OnPettingEnd(Level level)
    {

        Player entity = base.Scene.Tracker.GetEntity<Player>();
        if (entity != null)
        {
            entity.StateMachine.Locked = false;
            entity.StateMachine.State = 0;
        }
        pettingRoutine.Cancel();
        pettingRoutine.RemoveSelf();
        CatSprite.Play("idle");
        //CatSprite.Position = CatAnchor;
        if (PettingInProgress == true)
        {
            entity.Sprite.Position = friendposition;
        }
        entity.Sprite.Play("idle");
        //ThePetterSprite.Play("idle");
        ThePetterSprite.Visible = false;
        PettingInProgress = false;
    }

}
