using Godot;
using System;

public partial class Chair : Mover
{
    [Export]
    private Sprite2D _sprite;
    [Export]
    private Texture2D _textureLeft;
    [Export]
    private Texture2D _textureRight;
    [Export]
    private Texture2D _textureUp;
    [Export]
    private Texture2D _textureDown;
    [Export]
    public Vector2I Direction { get; set; } = new Vector2I(0, 1);

    public override void _Process(double delta)
    {
        _sprite.Texture = Direction == Vector2I.Left ? _textureLeft : _sprite.Texture;
        _sprite.Texture = Direction == Vector2I.Right ? _textureRight : _sprite.Texture;
        _sprite.Texture = Direction == Vector2I.Up ? _textureUp : _sprite.Texture;
        _sprite.Texture = Direction == Vector2I.Down ? _textureDown : _sprite.Texture;
    }

    public override bool CanMoveToward(Vector2I dir)
    {
        GD.Print($"Can Move Toward: {dir.X}, {dir.Y}");
        // return base.CanMoveToward(dir);
        if (IsWall(GridPosition + dir))
        {
            return false;
        }
        // Sit Chair
        if (dir * -1 == Direction)
        {
            Player.instance.IsSit = true;
            QueueFree();
        }
        return Direction == dir;
    }


}
