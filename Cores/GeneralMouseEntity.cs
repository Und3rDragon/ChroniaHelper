using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Settings;
using ChroniaHelper.Utils;
using Microsoft.Xna.Framework.Input;

namespace ChroniaHelper.Cores;

public class GeneralMouseEntity : Entity
{
    public GeneralMouseEntity(EntityData data, Vc2 offset) : base(data.Position + offset)
    {
        Tag |= Tags.Global;

        Instance = this;
    }
    public static GeneralMouseEntity Instance = null;

    [LoadHook]
    public static void Load()
    {
        On.Celeste.Level.Begin += OnLevelBegin;
        On.Celeste.Level.End += OnLevelEnd;
    }
    [UnloadHook]
    public static void Unload()
    {
        On.Celeste.Level.Begin -= OnLevelBegin;
        On.Celeste.Level.End -= OnLevelEnd;
    }

    public static void OnLevelBegin(On.Celeste.Level.orig_Begin orig, Level self)
    {
        orig(self);

        self.Add(Instance = new GeneralMouseEntity(new EntityData(), Vc2.Zero));

        Instance.Added(self);
    }

    public static void OnLevelEnd(On.Celeste.Level.orig_End orig, Level self)
    {
        Instance.Removed(self);
        Instance.RemoveSelf();

        self.Remove(Instance);

        orig(self);
    }

    public void StartDetecting()
    {
        Collider = new Hitbox(ButtonDetectionRadius * 2, ButtonDetectionRadius * 2,
            -ButtonDetectionRadius, -ButtonDetectionRadius);
    }

    public void EndDetecting()
    {
        Collider = null;
    }

    public override void Update()
    {
        Position = InputUtils.MouseLevelPosition;
    }

    public float ButtonDetectionRadius = 2f;
}
