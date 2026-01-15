using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Components;

[WorkingInProgress]
public class Passive3DCoordinates : BaseComponent
{
    public Vc2 InitialPosition;
    public Vc3 PassiveCoordinates;
    public float Fixation = 8f;
    private float Duration = 1f;

    public Passive3DCoordinates(Vc2 cubeCenter, float width, float height, float thickness, float duration = 1f)
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

    private bool rotating = false;
    private Vc3 initial, target;
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

            if(progress >= 1f)
            {
                rotating = false;
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
        while (rotating)
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

    public void RotateIn()
    {
        if (rotating) { return; }

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
        if (rotating) { return; }

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
        if (rotating) { return; }

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
        if (rotating) { return; }

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

    protected override void AfterEntityRemoved(Scene scene)
    {
        Entity.Position = InitialPosition;
    }
}
