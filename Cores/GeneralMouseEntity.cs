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

        if (MInput.Mouse.PressedLeftButton)
        {
            StartDetecting();

            leftButtonClick?.Invoke();

            EndDetecting();
        }

        if (MInput.Mouse.PressedRightButton)
        {
            StartDetecting();

            rightButtonClick?.Invoke();

            EndDetecting();
        }

        if (MInput.Mouse.PressedMiddleButton)
        {
            StartDetecting();

            middleButtonClick?.Invoke();

            EndDetecting();
        }

        if (MInput.Mouse.CheckLeftButton)
        {
            StartDetecting();

            leftButtonHold?.Invoke();

            EndDetecting();
        }

        if (MInput.Mouse.CheckMiddleButton)
        {
            StartDetecting();

            middleButtonHold?.Invoke();

            EndDetecting();
        }

        if (MInput.Mouse.CheckRightButton)
        {
            StartDetecting();

            rightButtonHold?.Invoke();

            EndDetecting();
        }

        if (!MInput.Mouse.CheckLeftButton)
        {
            StartDetecting();

            leftButtonEmpty?.Invoke();

            EndDetecting();
        }

        if (!MInput.Mouse.CheckMiddleButton)
        {
            StartDetecting();

            middleButtonEmpty?.Invoke();

            EndDetecting();
        }

        if (!MInput.Mouse.CheckRightButton)
        {
            StartDetecting();

            rightButtonEmpty?.Invoke();

            EndDetecting();
        }

        if (MInput.Mouse.ReleasedLeftButton)
        {
            StartDetecting();

            leftButtonRelease?.Invoke();

            EndDetecting();
        }

        if (MInput.Mouse.ReleasedMiddleButton)
        {
            StartDetecting();

            middleButtonRelease?.Invoke();

            EndDetecting();
        }

        if (MInput.Mouse.ReleasedRightButton)
        {
            StartDetecting();

            rightButtonRelease?.Invoke();

            EndDetecting();
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

    /// <summary>
    /// When using the actions, do "targetAction += newAction"
    /// </summary>
    public Action leftButtonClick, rightButtonClick, middleButtonClick,
        leftButtonHold, rightButtonHold, middleButtonHold,
        leftButtonEmpty, rightButtonEmpty, middleButtonEmpty,
        leftButtonRelease, rightButtonRelease, middleButtonRelease;
}
