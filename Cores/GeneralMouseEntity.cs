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

        Collider = new Hitbox(ButtonDetectionRadius * 2, ButtonDetectionRadius * 2,
            -ButtonDetectionRadius, -ButtonDetectionRadius);

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

    public override void Update()
    {
        Position = InputUtils.MouseLevelPosition;

        if (MInput.Mouse.PressedLeftButton)
        {
            leftButtonClick?.Invoke();
        }

        if (MInput.Mouse.PressedRightButton)
        {
            rightButtonClick?.Invoke();
        }

        if (MInput.Mouse.PressedMiddleButton)
        {
            middleButtonClick?.Invoke();
        }

        if (MInput.Mouse.CheckLeftButton)
        {
            leftButtonHold?.Invoke();
        }

        if (MInput.Mouse.CheckMiddleButton)
        {
            middleButtonHold?.Invoke();
        }

        if (MInput.Mouse.CheckRightButton)
        {
            rightButtonHold?.Invoke();
        }

        if (!MInput.Mouse.CheckLeftButton)
        {
            leftButtonEmpty?.Invoke();
        }

        if (!MInput.Mouse.CheckMiddleButton)
        {
            middleButtonEmpty?.Invoke();
        }

        if (!MInput.Mouse.CheckRightButton)
        {
            rightButtonEmpty?.Invoke();
        }

        if (MInput.Mouse.ReleasedLeftButton)
        {
            leftButtonRelease?.Invoke();
        }

        if (MInput.Mouse.ReleasedMiddleButton)
        {
            middleButtonRelease?.Invoke();
        }

        if (MInput.Mouse.ReleasedRightButton)
        {
            rightButtonRelease?.Invoke();
        }

        leftButtonClick = () => { };
        rightButtonClick = () => { };
        middleButtonClick = () => { };
        leftButtonHold = () => { };
        rightButtonHold = () => { };
        middleButtonHold = () => { };
        leftButtonRelease = () => { };
        rightButtonRelease = () => { };
        middleButtonRelease = () => { };
        leftButtonEmpty = () => { };
        rightButtonEmpty = () => { };
        middleButtonEmpty = () => { };
    }

    public float ButtonDetectionRadius = 2f;

    public Action leftButtonClick, rightButtonClick, middleButtonClick;
    public Action leftButtonHold, rightButtonHold, middleButtonHold;
    public Action leftButtonEmpty, rightButtonEmpty, middleButtonEmpty;
    public Action leftButtonRelease, rightButtonRelease, middleButtonRelease;
}
