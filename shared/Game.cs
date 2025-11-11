// Manage Movers, walls and undo/reset input

using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node
{
    public static Game Instance { get; private set; }

    public static List<Mover> Movers = new();

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
    }

    private void InitAfterFrame()
    {
        SetReferences();
        State.Init();
        foreach (var mover in Movers)
        {
            State.AddMover(mover);
        }
        State.AddToUndoStack();
    }

    private void SetReferences()
    {
        Movers.Clear();

        Movers = GetTree().GetNodesInGroup("movers").Cast<Mover>().ToList();
        
        GD.Print($"Find {Movers.Count} Movers: ");
        foreach (var mover in Movers)
        {
            GD.Print($"- {mover.Name} Pos: {mover.GridPosition}");
        }
    }
    
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("undo"))
        {
            holdingUndo = true;
            DoUndo();
        } else if (Input.IsActionJustPressed("reset"))
        {
            DoReset();
        }

        if (Input.IsActionJustReleased("undo"))
        {
            holdingUndo = false;
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
        State.DoReset();
        Refresh();
        Events.OnReset?.Invoke();
    }

    void DoUndo()
    {
        if (State.undoIndex <= 0)
        {
            return;
        }

        if (IsMoving)
        {
            CompleteMove();
        }
        State.DoUndo();
        Refresh();
        Events.OnUndo?.Invoke();
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

        for (int i = 0; i < 999 && Movers.Any(m => m.HasPlannedMove()); ++i)
        {
            PlannedMoves.Add(GetMoverPositions());
            bool isPushing = false;
            
            // Execute planned moves.
            var moved = false;
            foreach (var mover in Movers) {
                if (mover.ExecuteLogicalMove()) {
                    if (!mover.IsPlayer) isPushing = true;
                    moved = true;
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

        float duration = MoveTime / (Player.instance.InputBuffer.Count * MoveBufferSpeedupFactor + 1);
        foreach (var move in moves)
        {
            if (move.Pos == move.m.GridPosition)
                continue;

            _movingCount++;
            var targetPos = move.Pos;
            Tween tween = CreateTween();
            tween.TweenProperty(move.m, "position", move.m.Map.MapToLocal(targetPos), duration)
                .SetTrans(Tween.TransitionType.Sine);

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
        State.OnMoveComplete();
        Events.OnMoveComplete?.Invoke();
    }
}
