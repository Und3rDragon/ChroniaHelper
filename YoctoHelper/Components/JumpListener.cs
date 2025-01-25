using System;

namespace YoctoHelper.Components;

[Tracked(false)]
public class JumpListener : Component
{

    public Action<bool> OnJump { get; set; }

    public JumpListener(Action<bool> onJump) : base(active: false, visible: false)
    {
        this.OnJump = onJump;
    }

}
