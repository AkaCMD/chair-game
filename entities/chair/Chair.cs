using Godot;
using System;
using System.IO;

public partial class Chair : Mover
{
    [Export] private Sprite2D _sprite;

    // Chair textures
    [Export] private Texture2D _textureLeft;
    [Export] private Texture2D _textureRight;
    [Export] private Texture2D _textureUp;
    [Export] private Texture2D _textureDown;

    // Chair with box textures
    [Export] private Texture2D _textureBoxLeft;
    [Export] private Texture2D _textureBoxRight;
    [Export] private Texture2D _textureBoxUp;
    [Export] private Texture2D _textureBoxDown;

    [Export] public Vector2I Direction { get; set; } = Vector2I.Down;
    public bool HasBox { get; set; } = false;
    public Box BoxOnChair { get; set; }

    private const int MaxSlideAttempts = 20;
    private const float LevelExitDelay = 3.0f;

    public override void _Ready()
    {
        base._Ready();
        Events.OnPush += OnPushEvent;
    }

    private void OnPushEvent()
    {
        if (HasBox && Direction == Player.Instance.Direction)
        {
            HandleBoxSlide();
        }
    }

    private void HandleBoxSlide()
    {
        for (int slideAttempt = 0; slideAttempt < MaxSlideAttempts; slideAttempt++)
        {
            CommandManager.ExecuteCommand(new SlideChairCommand(this));

            if (IsTarget(GridPosition + Direction))
            {
                HandleTargetReached();
                break;
            }
        }
    }

    private void HandleTargetReached()
    {
        if (LevelSelector.Instance != null)
        {
            Events.OnLevelComplete.Invoke(LevelSelector.Instance.CurrentLevelName);
        }
        else
        {
            Events.OnLevelComplete.Invoke(Path.GetFileNameWithoutExtension(GetTree().CurrentScene.SceneFilePath));
        }
        var exitTimer = GetTree().CreateTimer(LevelExitDelay);

        Player.Instance.SoundCrush.Play();
        Player.Instance.SoundBreak.Play();
        Player.Instance.IsWaiting = true;
        LevelSelector.Instance.Break();

        exitTimer.Timeout += OnLevelExitTimerComplete;
    }

    private void OnLevelExitTimerComplete()
    {
        LevelSelector.OnLevelExit.Invoke(true);
    }

    public override void _Process(double delta)
    {
        UpdateChairTexture();
    }

    private void UpdateChairTexture()
    {
        if (HasBox)
        {
            SetBoxCarryingTexture();
        }
        else
        {
            SetNormalTexture();
        }
    }

    private void SetBoxCarryingTexture()
    {
        if (Direction == Vector2I.Left)
        {
            _sprite.Texture = _textureBoxLeft;
        }
        else if (Direction == Vector2I.Right)
        {
            _sprite.Texture = _textureBoxRight;
        }
        else if (Direction == Vector2I.Up)
        {
            _sprite.Texture = _textureBoxUp;
        }
        else if (Direction == Vector2I.Down)
        {
            _sprite.Texture = _textureBoxDown;
        }
    }

    private void SetNormalTexture()
    {
        if (Direction == Vector2I.Left)
        {
            _sprite.Texture = _textureLeft;
        }
        else if (Direction == Vector2I.Right)
        {
            _sprite.Texture = _textureRight;
        }
        else if (Direction == Vector2I.Up)
        {
            _sprite.Texture = _textureUp;
        }
        else if (Direction == Vector2I.Down)
        {
            _sprite.Texture = _textureDown;
        }
    }

    public override bool CanMoveToward(Vector2I direction)
    {
        if (!CanMoveInDirection(direction))
        {
            return false;
        }
        
        Mover adjacentMover = GetMover(GridPosition + direction);
        if (adjacentMover != null && adjacentMover != this)
        {
            Vector2I targetPos = adjacentMover.GridPosition + direction;
            
            if (IsWall(targetPos))
            {
                return false;
            }
            
            Mover targetMover = GetMover(targetPos);
            if (targetMover != null && targetMover != adjacentMover)
            {
                return false;
            }
            return adjacentMover.CanMoveToward(direction);
        }

        return true;
    }

    private bool CanMoveInDirection(Vector2I direction)
    {
        if (Player.Instance.HasBox)
        {
            return false;
        }

        if (IsWall(GridPosition + direction))
        {
            return false;
        }

        return Direction != -direction;
    }
}
