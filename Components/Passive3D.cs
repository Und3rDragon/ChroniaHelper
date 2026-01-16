using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using VivHelper.Triggers;

namespace ChroniaHelper.Components;

[WorkingInProgress]
public class Passive3D : BaseComponent
{
    public Vc2 InitialPosition;
    public Vc3 PassiveCoordinates;
    public float Fixation = 8f;
    private float Duration = 1f;

    public Passive3D(Vc2 cubeCenter, float width, float height, float duration = 1f)
    {
        InitialPosition = Entity.Position;
        Vc2 dP = Entity.Position - cubeCenter;
        PassiveCoordinates = new Vc3(dP.X, dP.Y, MathF.Log(Entity.Depth) * Fixation);
        Duration = duration.GetAbs();
    }

    public void Recalculate(float fixation = 8f)
    {
        Fixation = fixation;
        PassiveCoordinates.Z = MathF.Log(Entity.Depth) * Fixation;
    }

    private bool rotating = false, flatRotating = false, flatRotateClockwise = false;
    private Vc3 initial, target;
    private Vc2 flatInitial;
    private float progress = 0f;
    private Ease.Easer easer = Ease.Linear;
    public override void Update()
    {
        if (rotating)
        {
            if(Duration == 0f)
            {
                PassiveCoordinates = target;
                Entity.Position = new Vc2(PassiveCoordinates.X, PassiveCoordinates.Y);
                Entity.Depth = (int)Math.Pow(10, PassiveCoordinates.Z / Fixation);
                rotating = false;
                return;
            }

            progress += Engine.DeltaTime / Duration;
            PassiveCoordinates = initial.Approach(target, easer(progress));
            if(Entity is Platform p)
            {
                p.MoveTo(new Vc2(PassiveCoordinates.X, PassiveCoordinates.Y));
            }
            else
            {
                Entity.Position = new Vc2(PassiveCoordinates.X, PassiveCoordinates.Y);
            }
            Entity.Depth = (int)Math.Pow(10, PassiveCoordinates.Z / Fixation);

            if (progress >= 1f)
            {
                rotating = false;
                return;
            }
        }

        if (flatRotating)
        {
            if (Duration == 0f)
            {
                Vc2 flatTarget = flatInitial.Rotate(flatRotateClockwise ? 90f * Calc.DegToRad : -90f * Calc.DegToRad);
                PassiveCoordinates = new Vc3(flatTarget.X, flatTarget.Y, PassiveCoordinates.Z);
                Entity.Position = new Vc2(PassiveCoordinates.X, PassiveCoordinates.Y);
                Entity.Depth = (int)Math.Pow(10, PassiveCoordinates.Z / Fixation);
                flatRotating = false;
                return;
            }

            progress += Engine.DeltaTime / Duration;
            Vc2 needle = flatInitial.Rotate(90f * Ease.SineInOut(progress));
            PassiveCoordinates = new Vc3(needle.X, needle.Y, PassiveCoordinates.Z);
            if (Entity is Platform p)
            {
                p.MoveTo(new Vc2(PassiveCoordinates.X, PassiveCoordinates.Y));
            }
            else
            {
                Entity.Position = new Vc2(PassiveCoordinates.X, PassiveCoordinates.Y);
            }
            Entity.Depth = (int)Math.Pow(10, PassiveCoordinates.Z / Fixation);

            if (progress >= 1f)
            {
                flatRotating = false;
                return;
            }
        }
    }

    public void SetDuration(float duration = 1f)
    {
        Entity.Add(new Coroutine(AddingDuration(duration)));
    }

    private IEnumerator AddingDuration(float duration)
    {
        while (rotating || flatRotating)
        {
            yield return null;
        }

        Duration = duration;
    }

    public void InitiaizeSpin()
    {
        progress = 0f;
        rotating = false;
    }

    public void StartSpin()
    {
        progress = 0f;
        rotating = true;
    }

    public void StartFlatSpin()
    {
        progress = 0f;
        flatRotating = true;
    }

    public void RotateIn()
    {
        if (rotating || flatRotating) { return; }

        if(PassiveCoordinates.X == 0f)
        {
            easer = Ease.SineOut;
        }
        else if(PassiveCoordinates.Z == 0f)
        {
            easer = Ease.SineIn;
        }
        else
        {
            easer = Ease.SineInOut;
        }

        initial = PassiveCoordinates;
        target = new Vc3(-PassiveCoordinates.Z, PassiveCoordinates.Y, PassiveCoordinates.X);
        StartSpin();
    }

    public void RotateOut()
    {
        if (rotating || flatRotating) { return; }

        if (PassiveCoordinates.X == 0f)
        {
            easer = Ease.SineOut;
        }
        else if (PassiveCoordinates.Z == 0f)
        {
            easer = Ease.SineIn;
        }
        else
        {
            easer = Ease.SineInOut;
        }

        initial = PassiveCoordinates;
        target = new Vc3(PassiveCoordinates.Z, PassiveCoordinates.Y, -PassiveCoordinates.X);
        StartSpin();
    }

    public void RotateUp()
    {
        if (rotating || flatRotating) { return; }

        if (PassiveCoordinates.Y == 0f)
        {
            easer = Ease.SineOut;
        }
        else if (PassiveCoordinates.Z == 0f)
        {
            easer = Ease.SineIn;
        }
        else
        {
            easer = Ease.SineInOut;
        }

        initial = PassiveCoordinates;
        target = new Vc3(PassiveCoordinates.X, -PassiveCoordinates.Z, PassiveCoordinates.Y);
        StartSpin();
    }

    public void RotateDown()
    {
        if (rotating || flatRotating) { return; }

        if (PassiveCoordinates.Y == 0f)
        {
            easer = Ease.SineOut;
        }
        else if (PassiveCoordinates.Z == 0f)
        {
            easer = Ease.SineIn;
        }
        else
        {
            easer = Ease.SineInOut;
        }

        initial = PassiveCoordinates;
        target = new Vc3(PassiveCoordinates.X, PassiveCoordinates.Z, -PassiveCoordinates.Y);
        StartSpin();
    }

    public void RotateClockwise()
    {
        if (rotating || flatRotating) { return; }

        flatInitial = new Vc2(PassiveCoordinates.X, PassiveCoordinates.Y);
        flatRotateClockwise = true;
        StartFlatSpin();
    }

    public void RotateCounterclockwise()
    {
        if (rotating || flatRotating) { return; }

        flatInitial = new Vc2(PassiveCoordinates.X, PassiveCoordinates.Y);
        flatRotateClockwise = false;
        StartFlatSpin();
    }

    protected override void AfterEntityRemoved(Scene scene)
    {
        Entity.Position = InitialPosition;
    }
}
