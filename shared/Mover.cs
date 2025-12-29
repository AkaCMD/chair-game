// Things that can move that should be tracked for the undo system

using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Mover : Node2D
{
    public TileMapLayer Map;
    private Vector2I _gridPosition;

    public bool IsPlayer => IsInGroup("player");
    public bool IsSliding = false;

    // During a movement cycle, what's the next move (as a difference
    // from its current position) that this Mover will try to make?
    protected internal Vector2I _plannedMove;

    protected Vector2I prevMoveDir;

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
        GameEventSignals.Instance.MoveComplete += TrySlide;
    }

    public void Stop()
    {
        _plannedMove = Vector2I.Zero;
    }

    // Try to plan a move in the indicated direction, if that move is valid.
    public bool TryPlanMove(Vector2I dir)
    {
        Vector2I target = GridPosition + dir;
        if (!CanMoveToward(dir))
        {
            Bump(target);
            return false;
        }

        PlanMove(dir);
        return true;
    }

    protected virtual void PlanMove(Vector2I dir)
    {
        if (_plannedMove == dir) return;

        _plannedMove = dir;
        PlanPushes(dir);
    }

    public virtual bool HasPlannedMove()
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
    public virtual bool ExecuteLogicalMove()
    {
        if (_plannedMove == Vector2I.Zero)
        {
            return false;
        }

        CommandManager.ExecuteCommand(new MoveCommand(this));
        return true;
    }

    public void MoveIt()
    {
        _gridPosition += _plannedMove;
        prevMoveDir = _plannedMove;
        _plannedMove = Vector2I.Zero;
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
        if (IsCoffee(posToCheck, out _) && IsPlayer && !Player.Instance.IsSit)
        {
            return false;
        }

        // destructible obstacles
        if (m is Obstacle obstacle)
        {
            return obstacle.CanMoveToward(dir);
        }

        if (m is Coffee coffee)
        {
            return coffee.CanMoveToward(dir);
        }

        // Movers don't block themselves.
        if (m != null && m != this)
        {
            if (!IsPlayer && !Game.IsPolybanMode)
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

    public bool IsChair(Vector2I pos, out Chair chair)
    {
        foreach (var mover in GetAllMovers(pos))
        {
            if (mover is Chair chairObj)
            {
                chair = chairObj;
                return true;
            }
        }

        chair = null;
        return false;
    }

    public bool IsObstacle(Vector2I pos, out Obstacle obs)
    {
        if (GetMover(pos) != null && GetMover(pos).GetType() == typeof(Obstacle))
        {
            obs = (Obstacle)GetMover(pos);
            return true;
        }

        obs = null;
        return false;
    }

    public bool IsCoffee(Vector2I pos, out Coffee coffee)
    {
        foreach (var mover in GetAllMovers(pos))
        {
            if (mover is Coffee coffeeObj)
            {
                coffee = coffeeObj;
                return true;
            }
        }

        coffee = null;
        return false;
    }

    protected Mover GetMover(Vector2I pos)
    {
        foreach (var mover in GetTree().GetNodesInGroup("movers").OfType<Mover>())
        {
            if (mover.GridPosition == pos)
            {
                return mover;
            }
        }
        return null;
    }

    protected List<Mover> GetAllMovers(Vector2I pos)
    {
        var movers = new List<Mover>();
        foreach (var mover in GetTree().GetNodesInGroup("movers").OfType<Mover>())
        {
            if (mover.GridPosition == pos)
            {
                movers.Add(mover);
            }
        }

        return movers;
    }

    public void Bump(Vector2I targetGridPos, bool shouldMove = false)
    {
        Tween?.Kill();

        Vector2 currentPos = Map.MapToLocal(GridPosition);
        Vector2 targetPos = Map.MapToLocal(targetGridPos);

        Vector2 bumpPos = currentPos.Lerp(targetPos, 0.25f);

        Tween = CreateTween();
        Tween.SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        Tween.TweenProperty(this, "position", bumpPos, Game.Instance.MoveTime / 2);
        Tween.TweenProperty(this, "position", currentPos, Game.Instance.MoveTime / 2);

        if (shouldMove)
        {
            Tween.Finished += () =>
            {
                Game.Instance.MoveStart();
            };
        }
    }

    protected virtual void TrySlide()
    {
        if (IsSliding)
        {
            // Check if mover has fallen out of bounds (e.g., after breaking glass)
            if (GridPosition.X >= 500 || GridPosition.Y >= 500)
            {
                IsSliding = false;
                Player.Instance.SoundSlide.Stop();
                CommandManager.AddNewTurn();
                return;
            }

            IsSliding = TryPlanMove(prevMoveDir);
            if (IsSliding)
            {
                Player.Instance.SoundSlide.Stop();
                Utils.PlayWithRandomPitch(Player.Instance.SoundSlide);
                Game.Instance.MoveStart();
            }
            else
            {
                CommandManager.AddNewTurn();
            }
        }
    }

    public override void _ExitTree()
    {
        GameEventSignals.Instance.MoveComplete -= TrySlide;
        base._ExitTree();
    }
}
