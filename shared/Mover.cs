// Things that can move that should be tracked for the undo system

using System.Linq;
using Godot;

[GlobalClass]
public partial class Mover : Node2D
{
    private TileMapLayer _map;
    public Vector2I GridPosition { get; private set; }

    protected Tween Tween;

    public override void _Ready()
    {
        _map = GetParent<TileMapLayer>();
        GridPosition = _map.LocalToMap(Position);
        AddToGroup("movers");
    }

    public void MoveTo(Vector2I pos)
    {
        GridPosition = pos;
        if (Tween != null)
        {
            Tween.Kill();
        }

        Tween = CreateTween();
        Tween.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        Tween.TweenProperty(this, "position", _map.MapToLocal(pos), 0.2f);
    }

    protected void Bump(Vector2I pos)
    {
        if (Tween != null)
        {
            Tween.Kill();
        }
        
        Tween = CreateTween();
        Tween.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        Tween.TweenProperty(this, "position", (_map.MapToLocal(pos) + Position) / 2, 0.1f);
        Tween.TweenProperty(this, "position", _map.MapToLocal(GridPosition), 0.1f);
    }

    protected bool IsWall(Vector2I pos)
    {
        TileData data = _map.GetCellTileData(pos);
        if (data == null)
        {
            return false;
        }

        return data.GetCustomData("is_wall").AsBool();
    }

    protected bool IsTarget(Vector2I pos)
    {
        TileData data = _map.GetCellTileData(pos);
        if (data == null)
        {
            return false;
        }
        
        return data.GetCustomData("is_target").AsBool();
    }

    protected Box GetBox(Vector2I pos)
    {
        foreach (var box in GetTree().GetNodesInGroup("boxes").Cast<Box>())
        {
            if (box.GridPosition == pos)
            {
                return box;
            }
        }
        return null;
    }
}
