using System.Linq;
using Celeste.Mod.Entities;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Triggers;

[Tracked(true)]
[CustomEntity("ChroniaHelper/TeleportPositionTrigger")]
public class TeleportPositionTrigger : BaseTrigger
{

    protected string targetRoom;

    protected int targetPositionX;

    protected int targetPositionY;

    private float exitVelocityX;

    private float exitVelocityY;

    private BoolMode.BoolValue dreaming;

    private BoolMode.BoolValue firstLevel;

    private PlayerFacing.Facing playerFacing;

    private bool resetDashes;

    private bool clearState;

    private bool screenWipe;

    private bool velocityAsMultiplier = true;

    public TeleportPositionTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        base.enterIfFlag = FlagUtils.Parse(data.Attr("ifFlag", null));
        this.targetRoom = data.Attr("targetRoom", null);
        this.targetPositionX = data.Int("targetPositionX", -1);
        this.targetPositionY = data.Int("targetPositionY", -1);
        this.exitVelocityX = data.Float("exitVelocityX", 0F);
        this.exitVelocityY = data.Float("exitVelocityY", 0F);
        this.dreaming = data.Enum<BoolMode.BoolValue>("dreaming", BoolMode.BoolValue.None);
        this.firstLevel = data.Enum<BoolMode.BoolValue>("firstLevel", BoolMode.BoolValue.None);
        this.playerFacing = data.Enum<PlayerFacing.Facing>("playerFacing", PlayerFacing.Facing.None);
        this.resetDashes = data.Bool("resetDashes", true);
        this.clearState = data.Bool("clearState", true);
        this.screenWipe = data.Bool("screenWipe", false);
        this.velocityAsMultiplier = data.Bool("velocityAsMultiplier", true);
    }

    protected override void AddedExecute(Scene scene)
    {
        if (string.IsNullOrEmpty(this.targetRoom))
        {
            this.targetRoom = base.session.Level;
        }
    }

    protected override void OnEnterExecute(Player player)
    {
        base.level.OnEndOfFrame += delegate
        {
            if (this.targetPositionX < 0 || this.targetPositionY < 0)
            {
                return;
            }
            player.CleanUpTriggers();
            LevelData levelData = base.session.MapData.Get(this.targetRoom);
            int currentDashes = player.Dashes;
            Leader leader = player.Leader;
            foreach (Follower follower in leader.Followers.Where((Follower follower) => (follower.Entity != null)))
            {
                follower.Entity.Position -= player.TopLeft;
                follower.Entity.AddTag(Tags.Global);
                base.session.DoNotLoad.Add(follower.ParentEntityID);
            }
            for (int i = 0; i < leader.PastPoints.Count; i++)
            {
                leader.PastPoints[i] -= player.Position;
            }
            if (PlayerUtils.IsHeld(player))
            {
                player.Holding.Entity.AddTag(Tags.Global);
            }
            base.level.Remove(player);
            base.level.UnloadLevel();
            base.session.Level = this.targetRoom;
            base.level.Add(player);
            base.level.LoadLevel(Player.IntroTypes.Transition);
            player.Position = new Vector2(levelData.Position.X + this.targetPositionX, levelData.Position.Y + this.targetPositionY);
            if (velocityAsMultiplier)
            {
                player.Speed *= new Vector2(this.exitVelocityX, this.exitVelocityY);
            }
            else
            {
                player.Speed = new Vector2(this.exitVelocityX, this.exitVelocityY);
            }
            player.Dashes = (this.resetDashes ? player.MaxDashes : currentDashes);
            if (this.clearState)
            {
                player.DashDir = Vector2.Zero;
                player.StateMachine.State = 0;
            }
            PlayerFacing.Assignment(ref player.Facing, this.playerFacing);
            Vector2 cameraPosition = new Vector2();
            cameraPosition.X = Calc.Clamp(player.X - EngineUtils.ScreenHalfSize.X, base.level.Bounds.Left, (float) (base.level.Bounds.Right - EngineUtils.ScreenSize.X));
            cameraPosition.Y = Calc.Clamp(player.Y - EngineUtils.ScreenHalfSize.Y, base.level.Bounds.Top, (float) (base.level.Bounds.Bottom - EngineUtils.ScreenSize.Y));
            base.level.Camera.Position = cameraPosition;
            base.session.RespawnPoint = base.level.GetSpawnPoint(player.Position);
            foreach (Follower follower in leader.Followers.Where((Follower follower) => (follower.Entity != null)))
            {
                follower.Entity.Position += player.TopLeft;
                follower.Entity.RemoveTag(Tags.Global);
                base.session.DoNotLoad.Remove(follower.ParentEntityID);
            }
            for (int i = 0; i < leader.PastPoints.Count; i++)
            {
                leader.PastPoints[i] += player.Position;
            }
            leader.TransferFollowers();
            if (PlayerUtils.IsHeld(player))
            {
                player.Holding.Entity.RemoveTag(Tags.Global);
            }
            BoolMode.Assignment(ref base.session.Dreaming, this.dreaming);
            BoolMode.Assignment(ref base.session.FirstLevel, this.firstLevel);
            if (this.screenWipe)
            {
                level.DoScreenWipe(true, null, false);
            }
        };
    }

}
