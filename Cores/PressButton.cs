using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Cores;

public class PressButton : BaseSolid2
{
    public PressButton(EntityData data, Vc2 offset) : base(offset, data, 16, 6, false)
    {
        Collider = new Hitbox(16, 6, -8, 2);
    }

    public Button button;
    public bool currentButtonState = false;
    public bool previousButtonState = false;

    [Tracked(true)]
    public class Button : Entity
    {
        public Button(Vector2 position, string imagePath) : base(position)
        {
            Add(Image = new Image(GFX.Game[imagePath]));
            Collider = new Hitbox(14, 7, -7, -2);
            //Image.Position.X -= 6;
            //Image.Position.Y += 10;
            Image.Position.X -= Image.Width * 0.5f;
            Image.Position.Y -= Image.Height * 0.5f;
            imageY = Image.Position.Y;
        }

        public float imageY;
        public Image Image { get; private set; }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(button);
    }

    public override void Removed(Scene scene)
    {
        scene.Remove(button);
        base.Removed(scene);
    }

    public override void Update()
    {
        base.Update();
        List<Entity> actors = Scene.Tracker.GetEntities<Actor>();
        currentButtonState = false;
        foreach (Entity actor in actors)
        {
            if (Collide.Check(actor, button))
            {
                currentButtonState = true;
                break;
            }
        }

        if (currentButtonState)
        {
            WhenPressing();
        }
        else
        {
            WhenReleased();
        }

        if (currentButtonState != previousButtonState)
        {
            if (currentButtonState)
            {
                OnPress();
            }
            else
            {
                OnRelease();
            }
        }

        button.Image.Position.Y = Calc.Approach(button.Image.Position.Y,
            ButtonImageOffset(currentButtonState),
            40f * Engine.DeltaTime);

        previousButtonState = currentButtonState;
    }
    
    protected virtual float ButtonImageOffset(bool state = true)
    {
        return currentButtonState ? button.imageY + 2f : button.imageY;
    }
    
    protected virtual void WhenPressing() { }
    protected virtual void WhenReleased() { }
    protected virtual void OnPress() { }
    protected virtual void OnRelease() { }
}
