// Manage Movers, walls and undo/reset input

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node
{
    public static Game Instance { get; private set; }

    public List<Mover> Movers = new();

    public float MoveTime = 0.1f;  // time it takes to move 1 unit
    public float MoveBufferSpeedupFactor = 0.5f; // degree of speedup due to buffered inputs

    private int _movingCount = 0;
    private struct MoverPos
    {
        public Mover m;
        public Vector2I Pos;

        public MoverPos(Mover mov)
        {
            m = mov;
            Pos = m.GridPosition;
        }
    }
    private List<List<MoverPos>> PlannedMoves = new List<List<MoverPos>>();

    public bool holdingUndo { get; private set; } = false;
    public static bool isPolyban = true;
    public bool blockInput = false;

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
            GD.PushError("Multiple GameManager instances found.");
        }
    }

    public override void _Ready()
    {
        blockInput = true;
        CallDeferred("InitAfterFrame");
        blockInput = false;
        LevelSelector.OnLevelExit += (isBool) =>
        {
            blockInput = true;
            CallDeferred("InitAfterFrame");
            blockInput = false;
            Instance = null;
            QueueFree();
            GetTree().CallGroup("movers", "RemoveFromGroup", "movers");
        };
        Events.OnMoveComplete += SetReferences;
    }

    private void InitAfterFrame()
    {
        SetReferences();
        CommandManager.Init();
    }

    private void SetReferences()
    {
        if (!IsInstanceValid(this))
        {
            return;
        }
        
        Movers.Clear();

        Movers = GetTree().GetNodesInGroup("movers").Cast<Mover>().ToList();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("undo"))
        {
            if (!HasMoverSliding())
            {
                DoUndo();
                UndoRepeat();   
            }
        }

        if (Input.IsActionJustReleased("undo"))
        {
            StopUndoing();
        }

        if (Input.IsActionJustPressed("reset"))
        {
            DoReset();
        }
    }

    public void Refresh()
    {
        _movingCount = 0;
        PlannedMoves.Clear();
    }

    public bool IsMoving => _movingCount > 0;

    /////////////////////////////////////////////////////////////////// UNDO / RESET

    void DoReset()
    {
        CommandManager.ResetAll();
        Refresh();
        Events.OnReset?.Invoke();
    }

    void DoUndo()
    {
        Utils.PlayWithRandomPitch(Player.Instance.SoundUndo);
        if (IsMoving)
        {
            CompleteMove();
        }
        CommandManager.UndoCommand();

        Refresh();
        Events.OnUndo?.Invoke();
    }

    async void UndoRepeat()
    {
        holdingUndo = true;
        await ToSignal(GetTree().CreateTimer(GameConstants.UndoRepeatDelay), SceneTreeTimer.SignalName.Timeout);
        while (Input.IsActionPressed("undo") && holdingUndo)
        {
            DoUndo();
            await ToSignal(GetTree().CreateTimer(GameConstants.UndoRepeatDelay), SceneTreeTimer.SignalName.Timeout);
        }
    }

    async void StopUndoing()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        holdingUndo = false;
    }

    /////////////////////////////////////////////////////////////////// MOVE

    // Build a list of positions for each mover.
    private List<MoverPos> GetMoverPositions()
    {
        var lerps = new List<MoverPos>();
        foreach (var mover in Movers)
        {
            if (mover != null)
            {
                lerps.Add(new MoverPos(mover));
            }
        }

        return lerps;
    }


    // MoveStart calculates the 'logical' effects of a player action,
    // building up a list of movements. Those are executed visually afterward.
    public void MoveStart()
    {
        // For each movement 'cycle', we store the positions of all movers.
        PlannedMoves.Clear();
        Events.OnMoveStart?.Invoke(Player.Instance.Direction);

        for (int i = 0; i < GameConstants.MaxMovementCycles && Movers.Any(m => m.HasPlannedMove()); ++i)
        {
            PlannedMoves.Add(GetMoverPositions());
            bool isPushing = false;

            // Execute planned moves.
            var moved = false;
            foreach (var mover in Movers)
            {
                bool playerIsSitting = mover.IsPlayer && Player.Instance.IsSit;
                if (playerIsSitting)
                {
                    if (mover.ExecuteLogicalMove())
                    {
                        if (!mover.IsPlayer) isPushing = true;
                        moved = true;
                    }
                }
                else
                {
                    if (mover.ExecuteLogicalMove())
                    {
                        if (!mover.IsPlayer) isPushing = true;
                        moved = true;
                        Player.Instance.SoundWalk.Stop();
                        Utils.PlayWithRandomPitch(Player.Instance.SoundWalk);
                    }

                }
            }

            if (moved)
            {
                if (isPushing)
                {
                    Events.OnPush?.Invoke();
                }
            }
        }

        Player.Instance.IsPreviousSit = Player.Instance.IsSit;
        Player.Instance.PreviousPreviousDirection = Player.Instance.PreviousDirection;
        Player.Instance.PreviousDirection = Player.Instance.Direction;

        PlannedMoves.Add(GetMoverPositions());
        // Finally, start animating the moves we just calculated.
        StartMoveCycle();
        // After they're all done or cancelled, we'll run CompleteMove().

    }


    private void StartMoveCycle()
    {
        if (PlannedMoves.Count == 0)
        {
            CompleteMove();
            return;
        }

        var moves = PlannedMoves[0];
        PlannedMoves.RemoveAt(0);

        float duration = MoveTime / (Player.Instance.InputBuffer.Count * MoveBufferSpeedupFactor + 1);
        foreach (var move in moves)
        {
            if (move.Pos == move.m.GridPosition)
                continue;

            _movingCount++;
            var targetPos = move.Pos;
            Tween tween = CreateTween();
            tween.TweenProperty(move.m, "position", move.m.Map.MapToLocal(targetPos), duration)
                .SetTrans(Tween.TransitionType.Linear);

            tween.Finished += () =>
            {
                move.m.GridPosition = targetPos;
                MoveEnd();
            };
        }
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
        Events.OnMoveComplete?.Invoke();
    }

    public bool HasMoverSliding()
    {
        foreach (var mover in Movers)
        {
            if (mover.IsSliding)
                return true;
        }

        return false;
    }
}
