using Godot;

public partial class Obstacle : Mover
{
    [Export] private Sprite2D _sprite;
    [Export] private Texture2D _textureIntact;
    [Export] private Texture2D _textureBroken;

    private bool _isBroken = false;
    private bool _shouldBreakThisMove = false;
    
    [Export] public AudioStreamPlayer SoundBreak;

    public override void _Ready()
    {
        base._Ready();
        AddToGroup("obstacles");
        AddToGroup("destructible");
    }

    public override bool CanMoveToward(Vector2I dir)
    {
        if (_isBroken)
        {
            return true;
        }

        if (IsChairSlidingInto(dir))
        {
            _shouldBreakThisMove = true;
            return true;
        }

        return false;
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

    public override bool ExecuteLogicalMove()
    {
        bool hasMove = base.ExecuteLogicalMove();
        if (_shouldBreakThisMove)
        {
            CommandManager.ExecuteCommand(new BreakObstacleCommand(this));
            _shouldBreakThisMove = false;
        }
        return hasMove;
    }

    protected override void PlanMove(Vector2I dir)
    {
        _plannedMove = Vector2I.Zero;
    }

    public void Break()
    {
        SoundBreak.Stop();
        Utils.PlayWithRandomPitch(SoundBreak);
        
        _isBroken = true;
        _sprite.Texture = _textureBroken;
        YSortEnabled = false;
        ZIndex = 0;
        RemoveFromGroup("movers");

        // TODO: Play SFX
    }

    public void Restore()
    {
        _isBroken = false;
        _sprite.Texture = _textureIntact;
        YSortEnabled = true;
        ZIndex = 1;
        AddToGroup("movers");
    }
}
