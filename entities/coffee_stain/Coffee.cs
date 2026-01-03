using Godot;

public partial class Coffee : Node2D
{
    [Export] private Sprite2D _sprite;

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
    }
}
