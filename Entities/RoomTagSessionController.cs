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
[CustomEntity("ChroniaHelper/RoomTagSessionController")]
public class RoomTagSessionController : Entity
{
    public RoomTagSessionController(EntityData e, Vector2 offset): base(e.Position + offset)
    {
        setFlag = e.Bool("createFlag", true);
        setSlider = e.Attr("setSlider");
        setCounter = e.Attr("setCounter");
    }
    private bool setFlag;
    private string setSlider, setCounter;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        if (Md.Session.roomTagLoaded)
        {
            if (setFlag)
            {
                string source = MaP.session.LevelData.Name;
                for(int i = 0; i < Md.Session.rooms.Count; i++)
                {
                    if (source == Md.Session.rooms[i])
                    {
                        Md.Session.roomTags[i].SetFlag(true);
                    }
                    else
                    {
                        Md.Session.roomTags[i].SetFlag(false);
                    }
                }
            }
            if (!setSlider.IsNullOrEmpty())
            {
                string source = MaP.session.LevelData.Name;
                for (int i = 0; i < Md.Session.rooms.Count; i++)
                {
                    if (source == Md.Session.rooms[i])
                    {
                        Md.Session.roomTags[i].SetSlider(setSlider.ParseFloat(0f));
                    }
                }
            }
            if (!setCounter.IsNullOrEmpty())
            {
                string source = MaP.session.LevelData.Name;
                for (int i = 0; i < Md.Session.rooms.Count; i++)
                {
                    if (source == Md.Session.rooms[i])
                    {
                        Md.Session.roomTags[i].SetCounter(setSlider.ParseInt(0));
                    }
                }
            }
        }
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }
}
