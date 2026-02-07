// Manage Movers, walls and undo/reset input

using Godot;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public partial class Game : Node
{
    public static Game Instance { get; private set; }

    public List<Mover> Movers = new();

    public float MoveTime = 0.09f;  // time it takes to move 1 unit
    public float MoveBufferSpeedupFactor = 0.5f; // degree of speedup due to buffered inputs

    private int _movingCount = 0;

    public List<Step> StepHistory = new List<Step>();

    [Export] private CanvasLayer _gameOverOverlay;
    private bool _isGameOver = false;

    // Delegate fields for event subscriptions
    private GameEventSignals.LevelCompleteEventHandler _levelCompleteHandler;

    private struct MoverPosition
    {
        public Mover Mover;
        public Vector2I Position;

        public MoverPosition(Mover mover)
        {
            Mover = mover;
            Position = mover.GridPosition;
        }
    }

    private List<List<MoverPosition>> _plannedMoves = new List<List<MoverPosition>>();

    public bool IsHoldingUndo { get; private set; } = false;
    public static bool IsPolybanMode = true;
    public bool IsInputBlocked = false;
    public bool IsReplaying = false;

    [Export]
    public TileMapLayer ObjectsTileMapLayer;

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.PushError("Multiple Game instances found.");
        }
    }

    public override void _Ready()
    {
        IsInputBlocked = true;
        CallDeferred(nameof(InitAfterFrame));
        IsInputBlocked = false;

        LevelManager.Instance.OnLevelExit += OnLevelExit;
        GameEventSignals.Instance.MoveComplete += SetReferences;
        _levelCompleteHandler = levelName => GetNode<SaveManager>("/root/SaveManager").SubmitLevelClear(levelName, StepHistory);
        GameEventSignals.Instance.LevelComplete += _levelCompleteHandler;

        if (_gameOverOverlay != null)
        {
            _gameOverOverlay.Visible = false;   
        }
    }

    public override void _ExitTree()
    {
        LevelManager.Instance.OnLevelExit -= OnLevelExit;
        GameEventSignals.Instance.MoveComplete -= SetReferences;
        if (_levelCompleteHandler != null)
        {
            GameEventSignals.Instance.LevelComplete -= _levelCompleteHandler;
        }
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnLevelExit(bool isBool)
    {
        IsInputBlocked = true;
        CallDeferred(nameof(InitAfterFrame));
        IsInputBlocked = false;
        Instance = null;
        QueueFree();
        GetTree().CallGroup("movers", "RemoveFromGroup", "movers");
    }

    private void InitAfterFrame()
    {
        SetReferences();
    }

    private void SetReferences()
    {
        if (!IsInstanceValid(this)) return;

        Movers.Clear();
        Movers = GetTree().GetNodesInGroup("movers").OfType<Mover>().Where(m => m != null).ToList();
    }

    public override void _Process(double delta)
    {
        if (IsReplaying) return;
        HandleUndoInput();
        HandleResetInput();
    }

    public override void _Input(InputEvent @event)
    {
        // for test
        if (@event.IsActionPressed("ui_cancel"))
        {
            GetTree().ChangeSceneToFile("res://level_selector/level_selector.tscn");
        }

        if (@event.IsActionPressed("test"))
        {
            GetNode<ReplaySystem>("/root/ReplaySystem").StartReplay(LevelManager.Instance?.CurrentLevelName ?? Path.GetFileNameWithoutExtension(GetTree().CurrentScene.SceneFilePath));
        }
    }

    private void HandleUndoInput()
    {
        if (Input.IsActionJustPressed("undo") && !HasMoverSliding())
        {
            if (_isGameOver)
            {
                ResumeFromGameOver();
            }
            ExecuteUndo();
            StartUndoRepeat();   
        }

        if (Input.IsActionJustReleased("undo"))
        {
            StopUndoRepeat();
        }
    }

    private void HandleResetInput()
    {
        if (Input.IsActionJustPressed("reset"))
        {
            if (_isGameOver)
            {
                ResumeFromGameOver();
            }
            ExecuteReset();
        }
    }

    public void Refresh()
    {
        _movingCount = 0;
        _plannedMoves.Clear();
    }

    public bool IsMoving => _movingCount > 0;

    /////////////////////////////////////////////////////////////////// UNDO / RESET

    public void ExecuteReset()
    {
        KillAllTweens();
        StepHistory.Clear();
        CommandManager.ResetAll();
        Refresh();
        GameEventSignals.Instance.EmitSignal(GameEventSignals.SignalName.Reset);
    }

    private void ExecuteUndo()
    {
        KillAllTweens();
        if (IsMoving)
        {
            CompleteMove();
        }

        if (StepHistory.Count > 0)
        {
            StepHistory.RemoveAt(StepHistory.Count - 1);
        }
        Utils.PlayWithRandomPitch(Player.Instance.SoundUndo);

        CommandManager.UndoCommand();
        Refresh();
        GameEventSignals.Instance.EmitSignal(GameEventSignals.SignalName.Undo);
    }

    private async void StartUndoRepeat()
    {
        IsHoldingUndo = true;
        await ToSignal(GetTree().CreateTimer(GameConstants.UndoRepeatDelay), SceneTreeTimer.SignalName.Timeout);

        while (Input.IsActionPressed("undo") && IsHoldingUndo)
        {
            ExecuteUndo();
            await ToSignal(GetTree().CreateTimer(GameConstants.UndoRepeatDelay), SceneTreeTimer.SignalName.Timeout);
        }
    }

    private async void StopUndoRepeat()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        IsHoldingUndo = false;
    }

    /////////////////////////////////////////////////////////////////// MOVE

    private List<MoverPosition> GetCurrentMoverPositions()
    {
        var positions = new List<MoverPosition>();
        foreach (var mover in Movers)
        {
            if (mover != null)
            {
                positions.Add(new MoverPosition(mover));
            }
        }
        return positions;
    }

    public void MoveStart()
    {
        InitializeMovement();
        CalculateMovementCycles();
        UpdatePlayerState();
        StartMoveAnimation();
    }

    private void InitializeMovement()
    {
        _plannedMoves.Clear();
        GameEventSignals.Instance.EmitSignal(GameEventSignals.SignalName.MoveStart, Player.Instance.Direction);
    }

    private void CalculateMovementCycles()
    {
        for (int cycle = 0; cycle < GameConstants.MaxMovementCycles && Movers.Any(m => m.HasPlannedMove()); ++cycle)
        {
            _plannedMoves.Add(GetCurrentMoverPositions());
            ExecuteMovementCycle();
        }
    }

    private void ExecuteMovementCycle()
    {
        bool isPushing = false;
        bool moved = false;

        foreach (var mover in Movers)
        {
            if (TryExecuteMoverMove(mover, ref isPushing))
            {
                moved = true;
            }
        }

        if (moved && isPushing)
        {
            GameEventSignals.Instance.EmitSignal(GameEventSignals.SignalName.Push);
        }
    }

    private bool TryExecuteMoverMove(Mover mover, ref bool isPushing)
    {
        bool isPlayerSitting = mover.IsPlayer && Player.Instance.IsSit;

        if (mover.ExecuteLogicalMove())
        {
            if (!mover.IsPlayer)
                isPushing = true;

            if (!isPlayerSitting)
            {
                Player.Instance.SoundWalk.Stop();
                Utils.PlayWithRandomPitch(Player.Instance.SoundWalk);
            }
            return true;
        }
        return false;
    }

    private void UpdatePlayerState()
    {
        Player.Instance.IsPreviousSit = Player.Instance.IsSit;
        Player.Instance.PreviousPreviousDirection = Player.Instance.PreviousDirection;
        Player.Instance.PreviousDirection = Player.Instance.Direction;
        _plannedMoves.Add(GetCurrentMoverPositions());
    }

    private void StartMoveAnimation()
    {
        StartMoveCycle();
    }

    private void StartMoveCycle()
    {
        if (_plannedMoves.Count == 0)
        {
            CompleteMove();
            return;
        }

        var currentMoves = _plannedMoves[0];
        _plannedMoves.RemoveAt(0);

        float duration = CalculateMoveDuration();
        AnimateMovers(currentMoves, duration);
    }

    private float CalculateMoveDuration()
    {
        return MoveTime / (Player.Instance.InputBuffer.Count * MoveBufferSpeedupFactor + 1);
    }

    private void AnimateMovers(List<MoverPosition> moves, float duration)
    {
        foreach (var move in moves)
        {
            if (move.Position == move.Mover.GridPosition)
                continue;

            StartMoverAnimation(move, duration);
        }
    }

    private void StartMoverAnimation(MoverPosition move, float duration)
    {
        _movingCount++;
        var targetPos = move.Position;

        Tween tween = CreateTween();
        tween.TweenProperty(move.Mover, "position", move.Mover.Map.MapToLocal(targetPos), duration)
            .SetTrans(Tween.TransitionType.Linear);

        tween.Finished += () => OnMoverAnimationComplete(move.Mover, targetPos);
    }

    private void OnMoverAnimationComplete(Mover mover, Vector2I targetPosition)
    {
        mover.GridPosition = targetPosition;
        MoveEnd();
    }

    public void MoveEnd()
    {
        _movingCount--;
        if (_movingCount == 0)
        {
            StartMoveCycle();
        }
    }

    public void CompleteMove()
    {
        GameEventSignals.Instance.EmitSignal(GameEventSignals.SignalName.MoveComplete);
    }

    public bool HasMoverSliding()
    {
        return Movers.Any(mover => mover.IsSliding);
    }

    public void KillAllTweens()
    {
        foreach (var tween in GetTree().GetProcessedTweens())
        {
            if (tween.IsValid())
            {
                tween.Kill();
            }
        }
    }

    public void SetGameOver()
    {
        if (_isGameOver) return;
        
        _isGameOver = true;
        IsInputBlocked = true;
        if (_gameOverOverlay != null)
        {
            _gameOverOverlay.Visible = true;
        }
    }
    
    public void ResumeFromGameOver()
    {
        if (!_isGameOver) return;
        
        _isGameOver = false;
        IsInputBlocked = false;
        
        if (_gameOverOverlay != null)
        {
            _gameOverOverlay.Visible = false;
        }
    }
}
