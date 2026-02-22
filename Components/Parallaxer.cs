using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;

namespace ChroniaHelper.Components;

public class Parallaxer : BaseComponent
{
    public Parallaxer() { }

    public Parallaxer(Vc2 initial, Vc2 parallax)
    {
        InitialPosition = initial;
        Parallax = parallax;
        ApplyOnEntity = false;
    }

    public Parallaxer(Entity entity, Vc2 parallax)
    {
        Entity = entity;
        InitialPosition = entity.Position;
        Parallax = parallax;
        ApplyOnEntity = true;
    }

    private Vc2 InitialPosition;
    public Vc2 Parallax = Vc2.One;
    public bool ApplyOnEntity = false;

    public Vc2 ParallaxedPosition = Vc2.Zero;

    public override void Update()
    {
        if (Scene is not Level) { return; }

        Level level = Scene as Level;

        Vc2 cameraCenter = level.Camera.Position + new Vc2(160f, 90f);
        Vc2 diff = InitialPosition - cameraCenter;

        if (ApplyOnEntity)
        {
            //Entity.Position = cameraCenter + diff * Parallax;
            Entity.Position = InitialPosition + diff * (Parallax - Vc2.One);
        }

        //ParallaxedPosition = cameraCenter + diff * Parallax;
        ParallaxedPosition = InitialPosition + diff * (Parallax - Vc2.One);
    }
}
