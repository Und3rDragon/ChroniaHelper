using ChroniaHelper.Cores;

namespace ChroniaHelper.Components.SwimmingFish;

public class FishMotion : BaseComponent
{
    public Vc2 InitialPosition;
    public Vc2 Position;

    public Func<List<Vc2>> GetInterferePoints;

    public class Polygonal : FishMotion
    {
        public Polygonal(Vc2 startPosition, List<Vc2> borders)
        {
        
        }
    }

    public class MultiRectangular : FishMotion
    {
        public MultiRectangular(Vc2 startPosition, List<Rectangle> borders)
        {
            InitialPosition = Position = startPosition;
        }
    }
}