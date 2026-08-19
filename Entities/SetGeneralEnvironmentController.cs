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
            if (bloomBase.HasValidContent())
            {
                MaP.level.Bloom.Base = bloomBase.tryCreateSessionExpression().getFloatSessionExpressionValue();
            }

            if (bloomStrength.HasValidContent())
            {
                MaP.level.Bloom.Strength = bloomStrength.tryCreateSessionExpression().getFloatSessionExpressionValue();
            }

            if (lighting.HasValidContent())
            {
                float l = lighting.tryCreateSessionExpression().getFloatSessionExpressionValue();
                MaP.level.Session.LightingAlphaAdd = l - MaP.level.BaseLightingAlpha;
                MaP.level.Lighting.Alpha = MaP.level.BaseLightingAlpha + MaP.level.Session.LightingAlphaAdd;
            }
        }
        else
        {
            if (bloomBase.HasValidContent())
            {
                MaP.level.Bloom.Base = bloomBase.ParseMathExpression();
            }

            if (bloomStrength.HasValidContent())
            {
                MaP.level.Bloom.Strength = bloomStrength.ParseMathExpression();
            }

            if (lighting.HasValidContent())
            {
                float l = lighting.ParseMathExpression();
                MaP.level.Session.LightingAlphaAdd = l - MaP.level.BaseLightingAlpha;
                MaP.level.Lighting.Alpha = MaP.level.BaseLightingAlpha + MaP.level.Session.LightingAlphaAdd;
            }
        }
        
        if (bloomColor.HasValidContent())
        {
            if (Md.SaveData.chroniaColors.ContainsKey(bloomColor))
            {
                BloomTrigger.SetBloomColor(Md.SaveData.chroniaColors[bloomColor].Parsed());
            }
            else if (Md.Session.chroniaColors.ContainsKey(bloomColor))
            {
                BloomTrigger.SetBloomColor(Md.Session.chroniaColors[bloomColor].Parsed());
            }
        }
    }
}