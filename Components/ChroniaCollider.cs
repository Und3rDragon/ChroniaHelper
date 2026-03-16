using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Components;

public class ChroniaCollider : BaseComponent
{
    public class ColliderBuilder
    {
        public enum ColliderType
        {
            Hitbox, Circle
        }

        public ColliderBuilder(ColliderType type, List<SelectiveSessionValue> parameters)
        {
            Type = type;
            Parameters = parameters;
        }

        public ColliderType Type;
        public List<SelectiveSessionValue> Parameters;

        public Collider Build()
        {
            int N = Parameters.Count;

            if (N == 0) { return null; }

            if (Type == ColliderType.Circle)
            {
                float r = Parameters[0].GeneralValue;

                if (N >= 2)
                {
                    float x = Parameters[1].GeneralValue;

                    if (N >= 3)
                    {
                        float y = Parameters[2].GeneralValue;

                        return new Circle(r, x, y);
                    }

                    return new Circle(r, x);
                }

                return new Circle(r);
            }

            else if (Type == ColliderType.Hitbox)
            {
                if (N < 2) { return null; }

                float w = Parameters[0].GeneralValue;
                float h = Parameters[1].GeneralValue;

                if (N >= 3)
                {
                    float x = Parameters[2].GeneralValue;

                    if (N >= 4)
                    {
                        float y = Parameters[3].GeneralValue;

                        return new Hitbox(w, h, x, y);
                    }

                    return new Hitbox(w, h, x);
                }

                return new Hitbox(w, h);
            }

            return null;
        }
    }
    

    public List<ColliderBuilder> Colliders = new();
    public ChroniaCollider(params ColliderBuilder[] colliders)
    {
        Colliders = colliders.ToList();
    }

    private ColliderList BuildColliders()
    {
        ColliderList list = new();

        foreach(var collider in Colliders)
        {
            var c = collider.Build();

            if (c is not null) { list.Add(c); }
        }

        return list;
    }

    public override void Update()
    {
        Entity?.Collider = BuildColliders();
    }
}
