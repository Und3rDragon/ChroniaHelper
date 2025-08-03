using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using IL.MonoMod;
using Microsoft.Xna.Framework;
using Monocle;

// The source code of this entity is migrated from NeonHelper, which is integrated in City of Broken Dreams
// The original author is ricky06, code modified by UnderDragon


namespace ChroniaHelper.Entities.MigratedNeonHelper
{
	[Tracked(false)]
	public class PufferBombCollider : Component
	{
		public Action<PufferBomb> OnCollide;

		public Collider Collider;

		public PufferBombCollider(Action<PufferBomb> onCollide, Collider collider = null)
			: base(active: false, visible: false)
		{
			OnCollide = onCollide;
			Collider = null;
		}

		public void Check(PufferBomb puffer)
		{
			if (OnCollide != null)
			{
				Collider collider = Entity.Collider;
				if (Collider != null)
				{
					Entity.Collider = Collider;
				}
				if (puffer.CollideCheck(Entity))
				{
					OnCollide(puffer);
				}
				Entity.Collider = collider;
			}
		}
	}
}
