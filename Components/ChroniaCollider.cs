using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.MathExpression;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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

        public ColliderBuilder(ColliderType type, List<string> mathExpressions)
        {
            Type = type;
            MathExpressions = mathExpressions;
        }

        public ColliderType Type;
        public List<string> MathExpressions;

        public Collider Build()
        {
            int N = MathExpressions.Count;

            if (N == 0) { return null; }

            if (Type == ColliderType.Circle)
            {
                float r = MathExpressions[0].ParseMathExpression().GetAbs();

                if (r < 0.0001f) { return null; }

                if (N >= 2)
                {
                    float x = MathExpressions[1].ParseMathExpression();

                    if (N >= 3)
                    {
                        float y = MathExpressions[2].ParseMathExpression();

                        return new Circle(r, x, y);
                    }

                    return new Circle(r, x);
                }

                return new Circle(r);
            }

            else if (Type == ColliderType.Hitbox)
            {
                if (N < 2) { return null; }

                float w = MathExpressions[0].ParseMathExpression().GetAbs();
                float h = MathExpressions[1].ParseMathExpression().GetAbs();

                if(w < 0.00001f || h < 0.0001f) { return null; }

                if (N >= 3)
                {
                    float x = MathExpressions[2].ParseMathExpression();

                    if (N >= 4)
                    {
                        float y = MathExpressions[3].ParseMathExpression();

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
