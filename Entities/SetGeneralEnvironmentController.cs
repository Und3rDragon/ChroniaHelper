using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Imports;
using ChroniaHelper.Triggers;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.MathExpression;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/SetGeneralEnvironmentController")]
public class SetGeneralEnvironmentController : GeneralSetupController
{
    public SetGeneralEnvironmentController(EntityData data, Vc2 offset) : base(data, offset)
    {
        bloomBase = data.Attr("bloomBase");
        bloomStrength = data.Attr("bloomStrength");
        lighting = data.Attr("lighting");
        bloomColor = data.Attr("bloomColor");
        
        valueType = data.Int("valueType", 0);
    }

    private string bloomBase, bloomStrength, lighting, bloomColor;
    private int valueType = 0;
    public struct ValueType
    {
        public const int ChroniaExpression = 0;
        public const int FrostExpression = 1;
    }

    public override void Execute()
    {
        base.Execute();

        if (Md.FrostHelperLoaded && valueType == ValueType.FrostExpression)
        {
            if (bloomBase.HasValidContent() && bloomBase.tryCreateSessionExpression(out object exp))
            {
                MaP.SetBloomBase(exp.getFloatSessionExpressionValue());
            }

            if (bloomStrength.HasValidContent() && bloomStrength.tryCreateSessionExpression(out object exp1))
            {
                MaP.SetBloomStrength(exp1.getFloatSessionExpressionValue());
            }

            if (lighting.HasValidContent() && lighting.tryCreateSessionExpression(out object exp2))
            {
                float l = exp2.getFloatSessionExpressionValue();
                MaP.SetLightingAlpha(l);
            }
        }
        else
        {
            if (bloomBase.HasValidContent())
            {
                MaP.SetBloomBase(bloomBase.ParseMathExpression());
            }

            if (bloomStrength.HasValidContent())
            {
                MaP.SetBloomStrength(bloomStrength.ParseMathExpression());
            }

            if (lighting.HasValidContent())
            {
                float l = lighting.ParseMathExpression();
                MaP.SetLightingAlpha(l);
            }
        }
        
        if (bloomColor.HasValidContent())
        {
            if (Md.SaveData.chroniaColors.ContainsKey(bloomColor))
            {
                MaP.SetBloomColor(Md.SaveData.chroniaColors[bloomColor].Parsed());
            }
            else if (Md.Session.chroniaColors.ContainsKey(bloomColor))
            {
                MaP.SetBloomColor(Md.Session.chroniaColors[bloomColor].Parsed());
            }
        }
    }
}