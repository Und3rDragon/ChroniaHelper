namespace ChroniaHelper.Utils;

public class PlayerFacing
{

    public enum Facing
    {
        None,
        Left,
        Right
    }

    public static void Assignment(ref Facings value, PlayerFacing.Facing playerFacing)
    {
        if (playerFacing == PlayerFacing.Facing.None)
        {
            return;
        }
        value = (playerFacing == PlayerFacing.Facing.Left ? Facings.Left : Facings.Right);
    }

}
