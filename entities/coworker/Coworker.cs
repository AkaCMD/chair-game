using Godot;

public partial class Coworker : Mover
{
    [Export] private Sprite2D _sprite;
    [Export] private Texture2D _textureIntact;
    [Export] private Texture2D _textureBroken;

    public override void _Ready()
    {
        base._Ready();
        AddToGroup("obstacles");
    }

    public override bool CanMoveToward(Vector2I dir)
    {
        if (IsChairSlidingInto(dir))
        {
            return true;
        }

        return false;
    }
    
    protected override void PlanMove(Vector2I dir)
    {
        _plannedMove = Vector2I.Zero;
        // TODO: 播放同事躲闪动画
    }

    private bool IsChairSlidingInto(Vector2I dir)
    {
        Vector2I sourcePos = GridPosition - dir;
        Mover mover = GetMover(sourcePos);

        if (mover is Player player)
        {
            return player.IsSit;
        }

        if (mover is Chair chair)
        {
            return chair.HasBox;
        }

        return false;
    }
}