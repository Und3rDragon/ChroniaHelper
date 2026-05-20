using Celeste.Mod.Entities;
using ChroniaHelper.Components;
using ChroniaHelper.Components.StateListeners;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/OutlinerRectangle")]
[Tracked(true)]
[Credits("EnderallyGolem from Ender's Extras")]
[Note("The target is to combine the features of ScugHelper Outliner and Ender's Extras Outliner" +
    "Being highly customizable and attachable, ready for another series of indications")]
public class OutlinerRectangle : BaseEntity
{
    public List<OutlinerRectangle> group;
    public List<Image> imageList = [];
    public bool groupLeader = false;
    public Vector2 groupOrigin;
    public Wiggler wiggler;
    public Vector2 wigglerScaler;

    private FlagsListener visibleFlag;
    private CColor tintColor, fillerColor;
    private readonly int groupTag;
    private readonly bool attached;
    private readonly string texturePath;
    private readonly Vector2 nodeOffset = Vector2.Zero;

    private bool flagAllow = true;
    private bool previousFlagAllow = true;

    private InnerData.Float alphaFade, colorFade;
    private float fadeTime = -1f;
    private bool noFade = true;
    private EaseMode visibleFade = EaseMode.Linear;

    private string detectedColor, detectedFlag;
    private FlagsListener detectFlags;
    private bool detectActor, detectPlayer;

    public OutlinerRectangle(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        this.tintColor = data.GetChroniaColor("tintColor", "ffffffff");
        //this.fillerColor = data.GetChroniaColor("fillerColor", "ffffff33");

        visibleFlag = new(data.Attr("visibleFlag"));
        Add(visibleFlag);

        groupTag = data.Int("groupTag", 0);
        attached = data.Bool("attached", true);
        texturePath = data.Attr("texturePath", "ChroniaHelper/Outliner/outline_filled");

        Depth = data.Int("depth", 10000);
        Collider = new Hitbox(data.Width, data.Height);

        if (data.Nodes.Length > 0) nodeOffset = data.Nodes[0] + offset - Position;

        if (attached)
        {
            Add(new StaticMover
            {
                OnShake = OffsetImage,
                SolidChecker = IsRidingSolid,
                OnEnable = Miscs.EmptyAction,
                OnDisable = Miscs.EmptyAction,
                Visible = true,
            });
        }

        alphaFade = new($"fadeAlpha", 1f);
        Add(alphaFade);

        fadeTime = data.Float("displayFadeTime", -1f);
        noFade = fadeTime <= 0f;

        visibleFade = (EaseMode)data.Int("visibleFade", 1);

        detectedColor = data.Attr("detectedColor");
        detectedFlag = data.Attr("detectedFlag");
        colorFade = new("fadeColor", 0f);

        detectPlayer = data.Bool("detectPlayer", false);
        detectActor = data.Bool("detectActor", false);
        detectFlags = new(data.Attr("detectFlags"), fallback: false);
        Add(detectFlags);
    }

    public bool Detect()
    {
        bool b = false;

        b = b || detectFlags.InstantState;

        if (detectActor)
        {
            b = b || CollideCheck<Actor>();
        }
        else if(detectPlayer)
        {
            b = b || CollideCheck<Player>();
        }

        return b;
    }

    private bool IsRidingSolid(Solid solid)
    {
        Collider origCollider = base.Collider;
        base.Collider = new Hitbox(Width + 2, Height + 2, -1 + nodeOffset.X, -1 + nodeOffset.Y);
        bool collideCheck = CollideCheck(solid);
        base.Collider = origCollider;
        return collideCheck;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        // Connections!
        if (group == null)
        {
            groupLeader = true;
            group = new List<OutlinerRectangle>();
            group.Add(this);
            FindInGroup(this);
            float num = float.MaxValue;
            float num2 = float.MinValue;
            float num3 = float.MaxValue;
            float num4 = float.MinValue;
            foreach (OutlinerRectangle item in group)
            {
                if (item.Left < num) num = item.Left;
                if (item.Right > num2) num2 = item.Right;
                if (item.Bottom > num4) num4 = item.Bottom;
                if (item.Top < num3) num3 = item.Top;
            }

            groupOrigin = new Vector2((int)(num + (num2 - num) / 2f), (int)num4);
            wigglerScaler = new Vector2(Calc.ClampedMap(num2 - num, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(num4 - num3, 32f, 96f, 1f, 0.2f));
            Add(wiggler = Wiggler.Create(0.3f, 3f));
            foreach (OutlinerRectangle item2 in group)
            {
                item2.wiggler = wiggler;
                item2.wigglerScaler = wigglerScaler;
                item2.groupOrigin = groupOrigin;
            }
        }

        for (float num5 = base.Left; num5 < base.Right; num5 += 8f)
        {
            for (float num6 = base.Top; num6 < base.Bottom; num6 += 8f)
            {
                bool flag = CheckForSame(num5 - 8f, num6);
                bool flag2 = CheckForSame(num5 + 8f, num6);
                bool flag3 = CheckForSame(num5, num6 - 8f);
                bool flag4 = CheckForSame(num5, num6 + 8f);
                if (flag && flag2 && flag3 && flag4)
                {
                    if (!CheckForSame(num5 + 8f, num6 - 8f))
                    {
                        SetImage(num5, num6, 3, 0);
                    }
                    else if (!CheckForSame(num5 - 8f, num6 - 8f))
                    {
                        SetImage(num5, num6, 3, 1);
                    }
                    else if (!CheckForSame(num5 + 8f, num6 + 8f))
                    {
                        SetImage(num5, num6, 3, 2);
                    }
                    else if (!CheckForSame(num5 - 8f, num6 + 8f))
                    {
                        SetImage(num5, num6, 3, 3);
                    }
                    else
                    {
                        SetImage(num5, num6, 1, 1);
                    }
                }
                else if (flag && flag2 && !flag3 && flag4)
                {
                    SetImage(num5, num6, 1, 0);
                }
                else if (flag && flag2 && flag3 && !flag4)
                {
                    SetImage(num5, num6, 1, 2);
                }
                else if (flag && !flag2 && flag3 && flag4)
                {
                    SetImage(num5, num6, 2, 1);
                }
                else if (!flag && flag2 && flag3 && flag4)
                {
                    SetImage(num5, num6, 0, 1);
                }
                else if (flag && !flag2 && !flag3 && flag4)
                {
                    SetImage(num5, num6, 2, 0);
                }
                else if (!flag && flag2 && !flag3 && flag4)
                {
                    SetImage(num5, num6, 0, 0);
                }
                else if (flag && !flag2 && flag3 && !flag4)
                {
                    SetImage(num5, num6, 2, 2);
                }
                else if (!flag && flag2 && flag3 && !flag4)
                {
                    SetImage(num5, num6, 0, 2);
                }
            }
        }

        // Instant opacity change on awake
        alphaFade.Value = visibleFlag.InstantState ? 1f : 0f;
        colorFade.Value = Detect() ? 1f : 0f;
        Color init = tintColor.Parsed(alphaFade.Value);
        if (detectedColor.HasValidContent())
        {
            init = colorFade.Value.LerpValue(0f, 1f, tintColor.Parsed(alphaFade.Value),
                new CColor(detectedColor).Parsed(alphaFade.Value));
        }
        foreach (Image image in Components.GetAll<Image>())
        {
            image.Color = init;
        }
    }

    public void FindInGroup(OutlinerRectangle block)
    {
        foreach (OutlinerRectangle entity in base.Scene.Tracker.GetEntities<OutlinerRectangle>())
        {
            if (entity != this && entity != block && entity.groupTag == groupTag && entity.groupTag != -1
                && (entity.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) || entity.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))) && !group.Contains(entity))
            {
                group.Add(entity);
                FindInGroup(entity);
                entity.group = group;
            }
        }
    }

    public bool CheckForSame(float x, float y)
    {
        foreach (OutlinerRectangle entity in base.Scene.Tracker.GetEntities<OutlinerRectangle>())
        {
            if (entity.groupTag == groupTag && entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
            {
                return true;
            }
        }

        return false;
    }
    public void SetImage(float x, float y, int tx, int ty)
    {
        MTexture mtexture = GFX.Game[texturePath];
        imageList.Add(CreateImage(x, y, tx, ty, mtexture));
    }

    public Image CreateImage(float x, float y, int tx, int ty, MTexture tex)
    {
        Vector2 vector = new Vector2(x - base.X, y - base.Y);
        Image image = new Image(tex.GetSubtexture(tx * 8, ty * 8, 8, 8));
        Vector2 vector2 = groupOrigin - Position;
        image.Origin = vector2 - vector;
        image.Position = vector2;
        image.Color = tintColor.Parsed();
        Add(image);
        return image;
    }

    private bool detected = false, _detected = false;
    public override void Update()
    {
        visibleFlag.onEnable = () =>
        {
            float final = 1f;

            if (noFade)
            {
                alphaFade.Value = final;
            }
            else
            {
                alphaFade.FadeTo(final, fadeTime);
            }
            
        };

        visibleFlag.onDisable = () =>
        {
            float final = 0f;

            if (noFade)
            {
                alphaFade.Value = final;
            }
            else
            {
                alphaFade.FadeTo(final, fadeTime);
            }
        };

        detected = Detect();
        if(_detected != detected)
        {
            if (noFade)
            {
                colorFade.Value = detected ? 1f : 0f;
            }
            else
            {
                colorFade.FadeTo(detected ? 1f : 0f, fadeTime, visibleFade);
            }

            if (detectedFlag.HasValidContent())
            {
                detectedFlag.SetFlag(detected);
            }
        }

        Color set = tintColor.Parsed(alphaFade.Value);
        if (detectedColor.HasValidContent())
        {
            set = colorFade.Value.LerpValue(0f, 1f, tintColor.Parsed(alphaFade.Value),
                new CColor(detectedColor).Parsed(alphaFade.Value));
        }
        foreach (Image image in Components.GetAll<Image>())
        {
            image.Color = set;
        }

        _detected = detected;

        base.Update();
    }

    internal void OffsetImage(Vector2 offset)
    {
        foreach (Image image in Components.GetAll<Image>())
        {
            image.Position += offset;
        }
    }
}
