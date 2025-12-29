using Godot;
using System.Collections.Generic;

public partial class GlassWindow : Mover
{
    [Export] private Sprite2D _sprite;
    [Export] private Texture2D _textureIntact;
    [Export] private Texture2D _textureBroken;

    private bool _isBroken = false;
    private bool _shouldBreakThisMove = false;
    private Mover _breaker = null;
    private Vector2I _breakDirection = Vector2I.Zero;

    [Export] public AudioStreamPlayer SoundBreak;
    [Export] public AudioStreamPlayer SoundFall;

    private Dictionary<Mover, Vector2I> _fallenObjects = new Dictionary<Mover, Vector2I>();


    public override void _Ready()
    {
        base._Ready();
        AddToGroup("obstacles");
        AddToGroup("destructible");
        AddToGroup("windows");
    }

    public override bool CanMoveToward(Vector2I dir)
    {
        // If already broken, only allow sliding movers to pass through
        if (_isBroken)
        {
            // Check if the caller is sliding
            Mover caller = GetMover(GridPosition - dir);
            if (caller != null && caller.IsSliding)
            {
                // Sliding mover can pass through broken window and fall
                return true;
            }
            // Non-sliding movers cannot pass
            return false;
        }

        // Intact window: check if something is sliding into it
        if (IsSlidingInto(dir))
        {
            _shouldBreakThisMove = true;
            _breaker = GetMover(GridPosition - dir);
            _breakDirection = dir;
            return true;
        }

        // Otherwise, block like a wall
        return false;
    }
    
    private bool IsSlidingInto(Vector2I dir)
    {
        Vector2I sourcePos = GridPosition - dir;
        Mover mover = GetMover(sourcePos);

        // Case 1: Player sliding while sitting
        if (mover is Player player && player.IsSit)
        {
            return true;
        }

        // Case 2: Chair with box sliding
        if (mover is Chair chair && chair.HasBox)
        {
            return true;
        }
        
        return false;
    }
    
    public override bool ExecuteLogicalMove()
    {
        bool hasMove = base.ExecuteLogicalMove();
        if (_shouldBreakThisMove)
        {
            CommandManager.ExecuteCommand(new BreakWindowCommand(this, _breaker, _breakDirection));
            _shouldBreakThisMove = false;
        }
        return hasMove;
    }
    
    protected override void PlanMove(Vector2I dir)
    {
        _plannedMove = Vector2I.Zero;
    }
    
    public override bool HasPlannedMove()
    {
        return base.HasPlannedMove() || _shouldBreakThisMove;
    }
    
    public void Break(Mover breaker, Vector2I direction)
    {
        SoundBreak?.Stop();
        Utils.PlayWithRandomPitch(SoundBreak);
        
        _isBroken = true;
        _sprite.Texture = _textureBroken;
        
        // Keep in "movers" group so it still blocks non-sliding objects
        // Don't remove from group like Obstacle does
        
        // Trigger fall for breaker and any connected movers
        CauseFall(breaker, direction);
    }

    public void Restore()
    {
        _isBroken = false;
        _sprite.Texture = _textureIntact;
    }

    private void CauseFall(Mover mover, Vector2I direction)
    {
        if (mover == null) return;
        _fallenObjects.Clear();
        HashSet<Mover> fallenMovers = new HashSet<Mover>();
        CollectMovingGroup(mover, direction, fallenMovers);

        foreach (Mover m in fallenMovers)
        {
            // Store original position for undo
            _fallenObjects[m] = m.GridPosition;
            
            // Move to (999,999) to hide
            m.GridPosition = new Vector2I(999, 999);
            
            if (m is Player player)
            {
                // Player falls -> game over
                SoundFall?.Play();
                LevelManager.Instance?.EmitLevelExit(false);
                break; // No need to process others if player falls
            }
        }
    }

    private void CollectMovingGroup(Mover mover, Vector2I direction, HashSet<Mover> collected)
    {
        if (mover == null || collected.Contains(mover))
            return;
        
        collected.Add(mover);
        
        // Check if mover is planning to move in the same direction
        if (mover.HasPlannedMove() && mover._plannedMove == direction)
        {
            // Recursively collect movers that are being pushed by this mover
            Vector2I frontPos = mover.GridPosition + direction;
            List<Mover> frontMovers = GetAllMovers(frontPos);
            foreach (Mover front in frontMovers)
            {
                if (front != mover && front != this)
                {
                    CollectMovingGroup(front, direction, collected);
                }
            }
        }
    }
    
    // Method to restore fallen objects (for undo)
    public void RestoreFallenObjects()
    {
        foreach (var kvp in _fallenObjects)
        {
            Mover mover = kvp.Key;
            Vector2I originalPos = kvp.Value;
            mover.GridPosition = originalPos;
        }
        _fallenObjects.Clear();
    }
    
    // Getter for fallen objects (used by BreakWindowCommand)
    public Dictionary<Mover, Vector2I> GetFallenObjects()
    {
        return new Dictionary<Mover, Vector2I>(_fallenObjects);
    }
}
