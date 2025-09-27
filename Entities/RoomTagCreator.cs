using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Entities;

[Tracked(true)]
[CustomEntity("ChroniaHelper/RoomTagCreator")]
public class RoomTagCreator : Entity
{
    public RoomTagCreator(EntityData e, Vector2 offset): base(e.Position + offset)
    {
        tagExpression = e.Attr("setRoomTag", "&");
        forceLoad = e.Bool("forceLoad", false);
    }
    private string tagExpression;
    private bool forceLoad;

    public override void Added(Scene scene)
    {
        if(forceLoad || !Md.Session.roomTagLoaded)
        {
            Md.Session.rooms = new();
            Md.Session.roomTags = new();
            for (int i = 0; i < MaP.mapdata.Levels.Count; i++)
            {
                string name = MaP.mapdata.Levels[i].Name;
                Md.Session.rooms.Add(name);
                Md.Session.roomTags.Add(tagExpression.Replace("#", $"{i}").Replace("&", name));
            }
            Md.Session.roomTagLoaded = true;
        }

        base.Added(scene);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }
}
