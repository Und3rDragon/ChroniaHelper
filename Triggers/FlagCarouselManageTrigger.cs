using System.Collections.Generic;
using Celeste.Mod.Entities;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/FlagCarouselManageTrigger")]
public class FlagCarouselManageTrigger : Trigger
{

    private string carouselId;

    private CarouselMode carouselMode;

    public static Dictionary<string, FlagCarouselTrigger> CarouselDictionary
    {
        get => Md.Session.CarouselDictionary;
    }

    public enum CarouselMode
    {
        None,
        Pause,
        Resume,
        Cancel,
        Remove
    }

    public FlagCarouselManageTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        this.carouselId = data.Attr("carouselId");
        this.carouselMode = data.Enum<CarouselMode>("carouselMode", CarouselMode.None);
    }

    public override void OnEnter(Player player)
    {
        if (string.IsNullOrWhiteSpace(this.carouselId) || this.carouselMode == CarouselMode.None)
        {
            return;
        }
        FlagCarouselManageTrigger.CarouselDictionary.TryGetValue(this.carouselId, out FlagCarouselTrigger carousel);
        if (carousel == null)
        {
            return;
        }
        switch (this.carouselMode)
        {
            case CarouselMode.Pause:
                carousel.Pause();
                return;
            case CarouselMode.Resume:
                if (carousel.onPause)
                {
                    carousel.OnEnter(player);
                }
                return;
            case CarouselMode.Cancel:
                carousel.Cancel();
                return;
            case CarouselMode.Remove:
                carousel.Remove();
                FlagCarouselManageTrigger.CarouselDictionary.Remove(this.carouselId);
                return;
            default:
                return;
        }
    }

}
