using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;
using MonoMod.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Entities;

[CustomEntity("ChroniaHelper/LanguageSessionSpecifier")]
public class LanguageSessionSpecifier : BaseEntity
{
    public LanguageSessionSpecifier(EntityData data, Vc2 offset) : base(data, offset)
    {
        specifiers[Languages.English] = data.Attr("english");
        specifiers[Languages.Brazilian] = data.Attr("brazilian");
        specifiers[Languages.French] = data.Attr("french");
        specifiers[Languages.German] = data.Attr("german");
        specifiers[Languages.Italian] = data.Attr("italian");
        specifiers[Languages.Japanese] = data.Attr("japanese");
        specifiers[Languages.Korean] = data.Attr("korean");
        specifiers[Languages.Russian] = data.Attr("russian");
        specifiers[Languages.SimplifiedChinese] = data.Attr("simplifiedChinese");
        specifiers[Languages.Spanish] = data.Attr("spanish");

        sessionType = data.Int("sessionType", 0);
        name = data.Attr("targetName", "languageSessionValue");
    }
    public Dictionary<Language, string> specifiers = new();
    /// <summary>
    /// Flag = 0, Counter = 1, Slider = 2,
    /// </summary>
    public int sessionType = 0;
    public string name;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        Level level = SceneAs<Level>();

        var specifier = specifiers.GetValueOrDefault(Languages.Current, "");
        
        if(sessionType == 1 && specifier.TryParse(out int n))
        {
            name.SetCounter(n);
        }
        else if(sessionType == 2 && specifier.TryParse(out float f))
        {
            name.SetSlider(f);
        }
        else
        {
            if (specifier.HasValidContent())
            {
                name.SetFlag(true);
            }
        }
    }
}
