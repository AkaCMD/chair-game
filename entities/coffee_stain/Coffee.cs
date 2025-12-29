using Godot;

public partial class Coffee : Node2D
{
    [Export] private Sprite2D _sprite;
    [Export] private Texture2D _textureIntact;
    [Export] private Texture2D _textureBroken;

    public TileMapLayer Map;
    private Vector2I _gridPosition;

    public Vector2I GridPosition
    {
        get => _gridPosition;
        set
        {
            Position = Map.MapToLocal(value);
            _gridPosition = value;
        }
    }

    public override void _Ready()
    {
        Map = GetParent<TileMapLayer>();
        GridPosition = Map.LocalToMap(Position);
        AddToGroup("obstacles");
        // Do not add to "movers" group
    }

    // This method is called by other Movers to check if they can move into coffee
    public bool CanMoveToward(Vector2I dir)
    {
        if (IsChairSlidingInto(dir))
        {
            return true;
        }

        return false;
    }

    // Only chair and player(sit on chair) can be pushed into coffee
    private bool IsChairSlidingInto(Vector2I dir)
    {
        Vector2I sourcePos = GridPosition - dir;

        // Get movers at source position
        var movers = GetTree().GetNodesInGroup("movers");
        foreach (var node in movers)
        {
            if (node is Mover mover)
            {
                if (mover.GridPosition == sourcePos)
                {
                    if (mover is Player player)
                    {
                        return player.IsSit;
                    }

                    if (mover is Chair chair)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // Helper method to get mover at specific grid position
    private Mover GetMover(Vector2I pos)
    {
        foreach (var node in GetTree().GetNodesInGroup("movers"))
        {
            if (node is Mover mover && mover.GridPosition == pos)
            {
                return mover;
            }
        }
        return null;
    }
}
