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
    private Texture2D _textureBoxLeft;
    [Export]
    private Texture2D _textureBoxRight;
    [Export]
    private Texture2D _textureBoxUp;
    [Export]
    private Texture2D _textureBoxDown;
    [Export]
    public Vector2I Direction { get; set; } = new Vector2I(0, 1);
    public bool HasBox { get; set; } = false;
    public Box BoxOnChair { get; set; }


    public override void _Ready()
    {
        base._Ready();
        Events.OnPush += () =>
        {
            if (HasBox && Direction == Player.Instance.Direction)
            {
                for (int j = 0; j < 20; j++)
                {
                    CommandManager.ExecuteCommand(new SlideChairCommand(this));
                    if (IsTarget(GridPosition + Direction))
                    {
                        var timer = GetTree().CreateTimer(3);
                        Player.Instance.SoundCrush.Play();
                        Player.Instance.SoundBreak.Play();
                        Player.Instance.IsWaiting = true;
                        LevelSelector.Instance.Break();
                        timer.Timeout += () =>
                        {
                            LevelSelector.OnLevelExit.Invoke(true);
                        };
                    }
                }
            }
        };
    }


    public override void _Process(double delta)
    {
        if (HasBox)
        {
            _sprite.Texture = Direction == Vector2I.Left ? _textureBoxLeft : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Right ? _textureBoxRight : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Up ? _textureBoxUp : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Down ? _textureBoxDown : _sprite.Texture;
        }
        else
        {
            _sprite.Texture = Direction == Vector2I.Left ? _textureLeft : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Right ? _textureRight : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Up ? _textureUp : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Down ? _textureDown : _sprite.Texture;
        }
    }

    public override bool CanMoveToward(Vector2I dir)
    {
        if (HasBox)
        {
            Mover m = GetMover(GridPosition + dir);

            // Movers don't block themselves.
            if (m != null && m != this)
            {
                if (!m.CanMoveToward(dir))
                {
                    return false;
                }
            }
        }

        if (Player.Instance.HasBox)
        {
            return false;
        }
        if (dir * -1 == Direction && !HasBox)
        {
            CommandManager.ExecuteCommand(new SitChairCommand(this));
            CommandManager.AddNewTurn();
        }
        if (IsWall(GridPosition + dir))
        {
            return false;
        }

        return Direction == dir;
    }
}
