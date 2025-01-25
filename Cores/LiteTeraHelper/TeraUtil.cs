using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChroniaHelper.Cores.LiteTeraHelper
{
    internal static class TeraUtil
    {
        public static bool IsSuperEffective(TeraType Atk, TeraType Def)
        {
            if (!SuperEffectiveType.ContainsKey(Atk))
                return false;
            var set = SuperEffectiveType[Atk];
            return set.Contains(Def);
        }
        public static bool IsNotEffective(TeraType Atk, TeraType Def)
        {
            if (!NotEffectiveType.ContainsKey(Atk))
                return false;
            var set = NotEffectiveType[Atk];
            return set.Contains(Def);
        }
        public static bool IsNoEffect(TeraType Atk, TeraType Def)
        {
            if (!NoEffectType.ContainsKey(Atk))
                return false;
            var set = NoEffectType[Atk];
            return set.Contains(Def);
        }
        public static TeraEffect GetEffect(TeraType Atk, TeraType Def)
        {
            if (IsSuperEffective(Atk, Def))
                return TeraEffect.Super;
            if (IsNotEffective(Atk, Def))
                return TeraEffect.Bad;
            if (IsNoEffect(Atk, Def))
                return TeraEffect.None;
            return TeraEffect.Normal;
        }
        public static string GetImagePath(TeraType tera)
        {
            return "ChroniaHelper/objects/tera/Block/" + tera.ToString();
        }
        public static Color GetColor(TeraType tera)
        {
            return tera switch
            {
                /*
                TeraType.Bug => new Color(145, 161, 25),
                TeraType.Dragon => new Color(80, 96, 225),
                TeraType.Dark => new Color(80, 65, 63),
                TeraType.Fairy => new Color(239, 112, 239),
                TeraType.Electric => new Color(250, 192, 0),
                TeraType.Ground => new Color(145, 81, 33),
                
                TeraType.Flying => new Color(129, 185, 239),
                TeraType.Fighting => new Color(255, 128, 0),
                TeraType.Ice => new Color(63, 216, 255),
                TeraType.Steel => new Color(96, 161, 184),
                TeraType.Rock => new Color(175, 169, 129),
                TeraType.Psychic => new Color(239, 65, 121),
                TeraType.Poison => new Color(145, 65, 203),
                */

                TeraType.Ghost => new Color(112, 65, 112),
                TeraType.Grass => new Color(63, 161, 41),

                TeraType.Fire => new Color(230, 40, 41),

                TeraType.Normal => new Color(159, 161, 159),
                TeraType.Water => new Color(41, 128, 239),
                _ => Color.White,
            };
        }
        private static Dictionary<TeraType, HashSet<TeraType>> SuperEffectiveType = new Dictionary<TeraType, HashSet<TeraType>>
        {
            /*
            {TeraType.Fighting, new HashSet<TeraType>{TeraType.Normal, TeraType.Rock, TeraType.Ice, TeraType.Steel, TeraType.Dark } },
            {TeraType.Flying, new HashSet<TeraType>{TeraType.Bug, TeraType.Fighting, TeraType.Grass } },
            {TeraType.Poison, new HashSet<TeraType>{TeraType.Grass, TeraType.Fairy} },
            {TeraType.Ground, new HashSet<TeraType>{TeraType.Poison, TeraType.Fire, TeraType.Electric, TeraType.Rock, TeraType.Steel } },
            {TeraType.Rock, new HashSet<TeraType>{TeraType.Bug, TeraType.Flying, TeraType.Fire, TeraType.Ice } },
            {TeraType.Bug, new HashSet<TeraType>{TeraType.Grass, TeraType.Psychic, TeraType.Dark}},
            {TeraType.Ghost, new HashSet<TeraType>{TeraType.Psychic, TeraType.Ghost } },
            {TeraType.Steel, new HashSet<TeraType>{TeraType.Fairy, TeraType.Rock, TeraType.Ice} },
            {TeraType.Electric, new HashSet<TeraType>{TeraType.Flying, TeraType.Water} },
            {TeraType.Psychic, new HashSet<TeraType>{TeraType.Poison, TeraType.Fighting} },
            {TeraType.Ice, new HashSet<TeraType>{TeraType.Flying, TeraType.Ground, TeraType.Grass, TeraType.Dragon} },
            {TeraType.Dragon, new HashSet<TeraType>{TeraType.Dragon} },
            {TeraType.Dark, new HashSet<TeraType>{TeraType.Psychic, TeraType.Ghost } },
            {TeraType.Fairy, new HashSet<TeraType>{TeraType.Fighting, TeraType.Dragon, TeraType.Dark } },
            */

            /*  original setups
            {TeraType.Fire, new HashSet<TeraType>{TeraType.Grass, TeraType.Ice, TeraType.Bug, TeraType.Steel } },
            {TeraType.Water, new HashSet<TeraType>{TeraType.Ground, TeraType.Fire, TeraType.Rock} },
            {TeraType.Grass, new HashSet<TeraType>{TeraType.Ground, TeraType.Water, TeraType.Rock} },
            */

            {TeraType.Fire, new HashSet<TeraType>{TeraType.Grass } },
            {TeraType.Water, new HashSet<TeraType>{TeraType.Fire } },
            {TeraType.Grass, new HashSet<TeraType>{TeraType.Water } },
        };
        private static Dictionary<TeraType, HashSet<TeraType>> NotEffectiveType = new Dictionary<TeraType, HashSet<TeraType>>
        {
            /*
            {TeraType.Fighting, new HashSet<TeraType>{TeraType.Bug, TeraType.Poison, TeraType.Flying, TeraType.Psychic, TeraType.Fairy} },
            {TeraType.Flying, new HashSet<TeraType>{TeraType.Electric, TeraType.Rock, TeraType.Steel } },
            {TeraType.Poison, new HashSet<TeraType>{TeraType.Poison, TeraType.Ground, TeraType.Rock, TeraType.Ghost } },
            {TeraType.Ground, new HashSet<TeraType>{TeraType.Bug, TeraType.Grass } },
            {TeraType.Rock, new HashSet<TeraType>{TeraType.Ground, TeraType.Fighting, TeraType.Steel } },
            {TeraType.Bug, new HashSet<TeraType>{TeraType.Poison, TeraType.Flying, TeraType.Fighting, TeraType.Fire, TeraType.Ghost, TeraType.Steel, TeraType.Fairy} },
            {TeraType.Ghost, new HashSet<TeraType>{TeraType.Dark } },
            {TeraType.Steel, new HashSet<TeraType>{TeraType.Water, TeraType.Fire, TeraType.Electric, TeraType.Steel} },
            {TeraType.Electric, new HashSet<TeraType>{TeraType.Electric, TeraType.Grass, TeraType.Dragon} },
            {TeraType.Psychic, new HashSet<TeraType>{ TeraType.Psychic, TeraType.Steel} },
            {TeraType.Ice, new HashSet<TeraType>{TeraType.Fire, TeraType.Water, TeraType.Ice, TeraType.Steel} },
            {TeraType.Dragon, new HashSet<TeraType>{TeraType.Steel} },
            {TeraType.Dark, new HashSet<TeraType>{TeraType.Fighting, TeraType.Dark, TeraType.Fairy } },
            {TeraType.Fairy, new HashSet<TeraType>{TeraType.Poison, TeraType.Fire, TeraType.Steel} },
            */

            /* original setups
            {TeraType.Normal, new HashSet<TeraType> {TeraType.Rock, TeraType.Steel} },
            {TeraType.Fire, new HashSet<TeraType>{TeraType.Fire, TeraType.Water, TeraType.Rock, TeraType.Dragon} },
            {TeraType.Water, new HashSet<TeraType>{TeraType.Water, TeraType.Grass, TeraType.Dragon} },
            {TeraType.Grass, new HashSet<TeraType>{TeraType.Bug, TeraType.Poison, TeraType.Flying, TeraType.Fire, TeraType.Grass, TeraType.Dragon, TeraType.Steel} },
            */

            
            {TeraType.Fire, new HashSet<TeraType>{TeraType.Water } },
            {TeraType.Water, new HashSet<TeraType>{TeraType.Grass } },
            {TeraType.Grass, new HashSet<TeraType>{TeraType.Fire } },


        };
        private static Dictionary<TeraType, HashSet<TeraType>> NoEffectType = new Dictionary<TeraType, HashSet<TeraType>>
        {
            /* original setups
            {TeraType.Normal, new HashSet<TeraType>{TeraType.Ghost } },
            */
            {TeraType.Ghost, new HashSet<TeraType> {TeraType.Fire, TeraType.Water, TeraType.Grass, TeraType.Normal, TeraType.Ghost } },
            {TeraType.Fire, new HashSet<TeraType>{TeraType.Ghost } },
            {TeraType.Water, new HashSet<TeraType>{TeraType.Ghost } },
            {TeraType.Grass, new HashSet<TeraType>{TeraType.Ghost } },
            {TeraType.Normal, new HashSet<TeraType>{TeraType.Ghost } },
            /*
            {TeraType.Fighting, new HashSet<TeraType>{TeraType.Ghost} },
            {TeraType.Poison, new HashSet<TeraType>{TeraType.Steel} },
            {TeraType.Ground, new HashSet<TeraType>{TeraType.Flying } },
            {TeraType.Ghost, new HashSet<TeraType>{TeraType.Normal } },
            {TeraType.Electric, new HashSet<TeraType> {TeraType.Ground} },
            {TeraType.Psychic, new HashSet<TeraType>{TeraType.Dark} },
            {TeraType.Dragon, new HashSet<TeraType>{TeraType.Fairy} },
            */
        };
    }
}
