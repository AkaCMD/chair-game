using System.Collections.Generic;
using Godot;
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
    [Export] public bool HasBox { get; set; } = false;
    public Box BoxOnChair { get; set; }

    private const float LevelExitDelay = 3.0f;

    // Dialog
    private bool _isWaitingForDialog = false;

    // Box sliding state
    private bool _isBoxSliding = false;

    public override void _Ready()
    {
        base._Ready();
        GameEventSignals.Instance.Push += OnPushEvent;

        if (HasBox)
        {
            var boxScene = GD.Load<PackedScene>("res://entities/box/box.tscn");
            if (boxScene != null)
            {
                var boxInstance = boxScene.Instantiate<Box>();
                Map.AddChild(boxInstance);
                BoxOnChair = boxInstance;
                BoxOnChair.GridPosition = new Vector2I(999, 999);
            }
        }
    }

    private void OnPushEvent()
    {
        if (_isBoxSliding) return; // Prevent recursive calls
        if (HasBox && 
            Direction == Player.Instance.Direction && 
            Player.Instance.GridPosition == GridPosition - Direction && 
            !Player.Instance.IsSit && 
            !Player.Instance.HasBox)
        {
            Game.Instance.StepHistory.Add(Step.CreateMove(Direction));
            Player.Instance.PrintSolutionSequence();
            StartBoxSlide(); // Replace HandleBoxSlide with event-driven sliding
        }
    }

    private void StartBoxSlide()
    {
        if (_isBoxSliding) return;

        _isBoxSliding = true;
        IsSliding = true;
        prevMoveDir = Direction;

        // Execute first slide move
        ExecuteBoxSlideMove();
    }

    private void ExecuteBoxSlideMove()
    {
        // Use command to record movement (maintain undo support)
        CommandManager.ExecuteCommand(new SlideChairCommand(this));
    }

    private void ReplaceDoorWithBrokenVersion(Vector2I gridPosition)
    {
        Vector2I downCell = new Vector2I(9, 1); // hard coded
        Vector2I upCell = new Vector2I(9, 0);
        Map.SetCell(gridPosition, Map.GetCellSourceId(downCell), downCell);
        Map.SetCell(gridPosition + Vector2I.Up, Map.GetCellSourceId(upCell), upCell);
    }

    private void HandleTargetReached()
    {
        string levelName = LevelManager.Instance?.CurrentLevelName;
        if (string.IsNullOrEmpty(levelName))
        {
            levelName = Path.GetFileNameWithoutExtension(GetTree().CurrentScene.SceneFilePath);
        }

        Player.Instance.SoundCrush.Play();
        Player.Instance.SoundBreak.Play();
        Player.Instance.IsWaiting = true;
        if (LevelSelector.Instance != null) LevelSelector.Instance.Break();

        if (levelName == "beginning")
        {
            PlayDialogsBeforeCompleted();
        }
        else
        {
            CompleteLevelAndExit();
        }
    }

    private void OnLevelExitTimerComplete()
    {
        LevelManager.Instance.EmitLevelExit(true);
        // 当不从 LevelSelector 场景进入而是单独测试关卡的时候，过关不播放 Break 动画，直接进入 LevelSelector
        if (LevelSelector.Instance == null)
        {
            GetTree().ChangeSceneToFile("res://level_selector/level_selector.tscn");
        }
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
            if (IsCoffee(targetPos, out Coffee _))
            {
                return true;
            }

            Mover targetMover = GetMover(targetPos);
            if (targetMover != null && targetMover != adjacentMover)
            {
                return targetMover.CanMoveToward(direction);
            }
            return adjacentMover.CanMoveToward(direction);
        }

        return true;
    }

    private bool CanMoveInDirection(Vector2I direction)
    {
        if (Player.Instance.HasBox) return false;
        
        if (HasBox && Direction != direction) return false;
        
        if (IsWall(GridPosition + direction)) return false;
        
        if (IsCoffee(GridPosition + direction, out _)) return true;
        
        return HasBox || Direction != -direction;
    }

    public override void _ExitTree()
    {
        GameEventSignals.Instance.Push -= OnPushEvent;
        base._ExitTree();
    }

    protected override void TrySlide()
    {
        if (_isBoxSliding)
        {
            // Check if chair has fallen out of bounds (e.g., after breaking glass)
            if (GridPosition.X >= 500 || GridPosition.Y >= 500)
            {
                StopBoxSlide();
                CommandManager.AddNewTurn();
                return;
            }

            // Check if reached target
            if (IsTarget(GridPosition + prevMoveDir))
            {
                HandleTargetReached();
                ReplaceDoorWithBrokenVersion(GridPosition + prevMoveDir);
                StopBoxSlide();
                return;
            }

            // Check for obstacles
            if (IsObstacle(GridPosition + prevMoveDir, out Obstacle obs))
            {
                CommandManager.ExecuteCommand(new BreakObstacleCommand(obs));
            }

            // Try to continue sliding
            IsSliding = TryPlanMove(prevMoveDir);
            if (IsSliding)
            {
                Player.Instance.SoundSlide.Stop();
                Utils.PlayWithRandomPitch(Player.Instance.SoundSlide);
                Game.Instance.MoveStart();
            }
            else
            {
                StopBoxSlide();
                CommandManager.AddNewTurn();
            }
        }
        else
        {
            base.TrySlide(); // Keep original player-on-chair sliding logic
        }
    }

    private void StopBoxSlide()
    {
        _isBoxSliding = false;
        IsSliding = false;
        Player.Instance.SoundSlide.Stop();
    }

    private void PlayDialogsBeforeCompleted()
    {
        DialogController.Instance.StartDialog(new List<DialogResource>
        {
            GD.Load<DialogResource>("res://dialog/dialog_resources/beginning/dialog_beginning_02.tres"),
            GD.Load<DialogResource>("res://dialog/dialog_resources/beginning/dialog_beginning_03.tres"),
            GD.Load<DialogResource>("res://dialog/dialog_resources/beginning/dialog_beginning_04.tres")
        }, "beginning_transition");
        GameEventSignals.Instance.DialogComplete += OnTransitionDialogComplete;
        _isWaitingForDialog = true;
    }

    private void OnTransitionDialogComplete(string dialogId = "")
    {
        if (dialogId == "beginning_transition")
        {
            GameEventSignals.Instance.DialogComplete -= OnTransitionDialogComplete;
            _isWaitingForDialog = false;
            CompleteLevelAndExit();
        }
    }

    private void CompleteLevelAndExit()
    {
        string levelName = LevelManager.Instance?.CurrentLevelName;
        if (string.IsNullOrEmpty(levelName))
        {
            levelName = Path.GetFileNameWithoutExtension(GetTree().CurrentScene.SceneFilePath);
        }
        GameEventSignals.Instance.EmitSignal(GameEventSignals.SignalName.LevelComplete, levelName);
        var exitTimer = GetTree().CreateTimer(LevelExitDelay);
        exitTimer.Timeout += OnLevelExitTimerComplete;
    }
}
