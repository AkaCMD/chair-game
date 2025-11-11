// Things that can move that should be tracked for the undo system

using System.Linq;
using Godot;

[GlobalClass]
public partial class Mover : Node2D
{
    public TileMapLayer Map;
    private Vector2I _gridPosition;

    public bool IsPlayer => IsInGroup("player");
    
    // During a movement cycle, what's the next move (as a difference
    // from its current position) that this Mover will try to make?
    private Vector2I _plannedMove;

    public Vector2I GridPosition
    {
        get => _gridPosition;
        set
        {
            Position = Map.MapToLocal(value);
            _gridPosition = value;
        }
    }

    protected Tween Tween;

    public override void _Ready()
    {
        Map = GetParent<TileMapLayer>();
        GridPosition = Map.LocalToMap(Position);
        AddToGroup("movers");
    }

    public void Stop()
    {
        _plannedMove = Vector2I.Zero;
    }

    // Try to plan a move in the indicated direction, if that move is valid.
    public bool TryPlanMove(Vector2I dir)
    {
        if (!CanMoveToward(dir))
        {
            return false;
        }

        PlanMove(dir);
        return true;
    }

    private void PlanMove(Vector2I dir)
    {
        if (_plannedMove == dir) return;

        _plannedMove = dir;
        PlanPushes(dir);
    }

    public bool HasPlannedMove()
    {
        return _plannedMove != Vector2I.Zero;
    }
    
    // If there are other movers in the given direction,
    // push them in the same direction.
    private void PlanPushes(Vector2I dir)
    {
        // TODO: 如果该mover包含多格，需要单独检查每个tile的方向
        Vector2I posToCheck = GridPosition + dir;
        Mover m = GetMover(posToCheck);
        if (m == null || m == this) return;
        m.PlanMove(dir);
    }
    
    // Perform the currently planned move (if any).
    public bool ExecuteLogicalMove()
    {
        if (_plannedMove == Vector2I.Zero)
        {
            return false;
        }

        _gridPosition += _plannedMove;
        _plannedMove = Vector2I.Zero;
        return true;
    }

    public virtual bool CanMoveToward(Vector2I dir)
    {
        // TODO: 如果该mover包含多格，需要单独检查每个tile的方向
        Vector2I posToCheck = GridPosition + dir;
        if (IsWall(posToCheck))
        {
            return false;
        }
        Mover m = GetMover(posToCheck);
        
        // Movers don't block themselves.
        if (m != null && m != this)
        {
            if (!IsPlayer && !Game.isPolyban)
            {
                return false;
            }
            if (!m.CanMoveToward(dir))
            {
                return false;
            }
        }

        return true;
    }
    
    protected bool IsWall(Vector2I pos)
    {
        TileData data = Map.GetCellTileData(pos);
        if (data == null)
        {
            return false;
        }

        return data.GetCustomData("is_wall").AsBool();
    }

    protected bool IsTarget(Vector2I pos)
    {
        TileData data = Map.GetCellTileData(pos);
        if (data == null)
        {
            return false;
        }
        
        return data.GetCustomData("is_target").AsBool();
    }

    protected Mover GetMover(Vector2I pos)
    {
        foreach (var mover in GetTree().GetNodesInGroup("movers").Cast<Mover>())
        {
            if (mover.GridPosition == pos)
            {
                return mover;
            }
        }
        return null;
    }
}
