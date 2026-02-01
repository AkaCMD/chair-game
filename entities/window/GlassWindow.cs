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

        if (mover is Turret turret)
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
        if (!_isBroken)
        {
            SoundBreak?.Stop();
            Utils.PlayWithRandomPitch(SoundBreak);

            _isBroken = true;
            _sprite.Texture = _textureBroken;
        }

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

        if (TryHandleBossFightFall(fallenMovers))
        {
            return;
        }

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
                if (Game.Instance != null)
                {
                    Game.Instance.SetGameOver();
                }
                break; // No need to process others if player falls
            }
        }
    }

    private bool TryHandleBossFightFall(HashSet<Mover> fallenMovers)
    {
        string currentLevel = LevelManager.Instance?.CurrentLevelName;
        if (string.IsNullOrEmpty(currentLevel))
        {
            currentLevel = System.IO.Path.GetFileNameWithoutExtension(GetTree().CurrentScene.SceneFilePath);
        }

        if (currentLevel == "boss_fight")
        {
            return HandleBossFightFall(fallenMovers);
        }

        return false;
    }

    private bool HandleBossFightFall(HashSet<Mover> fallenMovers)
    {
        bool playerFell = false;
        bool bossFell = false;

        foreach (var m in fallenMovers)
        {
            if (m is Player)
            {
                playerFell = true;
            }
            else if (m is Turret turret)
            {
                // In boss_fight level, treat any Turret as the boss
                bossFell = true;
            }
        }

        foreach (Mover m in fallenMovers)
        {
            _fallenObjects[m] = m.GridPosition;
            m.GridPosition = new Vector2I(999, 999);
        }

        // Handle based on what fell
        if (playerFell)
        {
            SoundFall?.Play();
            if (Game.Instance != null)
            {
                Game.Instance.SetGameOver();
            }
            return true;
        }
        else if (bossFell)
        {
            SoundFall?.Play();

            // show CG
            CanvasLayer cgLayer = null;

            if (LevelSelector.Instance != null)
            {
                cgLayer = GetNodeOrNull<CanvasLayer>("/root/LevelSelector/CanvasLayer/Main/GUI/CgLayer");
            }
            else
            {
                cgLayer = GetNodeOrNull<CanvasLayer>("/root/Main/GUI/CgLayer");
            }
            cgLayer.Show();

            // Show credits in the CgLayer
            var creditsController = cgLayer.GetNodeOrNull<CreditsController>("CreditsDisplay");
            if (creditsController != null)
            {
                creditsController.StartCredits();
            }

            GameEventSignals.Instance.EmitSignal(GameEventSignals.SignalName.LevelComplete, "boss_fight");

            // Wait for credits completion via signal instead of timer
            if (creditsController != null)
            {
                creditsController.CreditsFinished += OnCreditsFinished;
            }
            else
            {
                // Fallback: if credits controller not found, use timer
                var exitTimer = GetTree().CreateTimer(15.0f);
                exitTimer.Timeout += () =>
                {
                    OnCreditsFinished();
                };
            }
            return true;
        }

        return false;
    }

    private void OnCreditsFinished()
    {
        // Safety check: ensure the scene hasn't been unloaded
        if (!IsInstanceValid(this))
        {
            return;
        }

        // Find CgLayer again since it might have changed
        CanvasLayer cgLayer = null;
        if (LevelSelector.Instance != null)
        {
            cgLayer = GetNodeOrNull<CanvasLayer>("/root/LevelSelector/CanvasLayer/Main/GUI/CgLayer");
        }
        else
        {
            cgLayer = GetNodeOrNull<CanvasLayer>("/root/Main/GUI/CgLayer");
        }

        if (IsInstanceValid(cgLayer))
        {
            cgLayer.Hide();
        }

        // Clean up signal connection
        if (IsInstanceValid(cgLayer))
        {
            var creditsController = cgLayer.GetNodeOrNull<CreditsController>("CreditsDisplay");
            if (IsInstanceValid(creditsController))
            {
                creditsController.CreditsFinished -= OnCreditsFinished;
            }
        }

        if (LevelSelector.Instance == null)
        {
            GetTree().ChangeSceneToFile("res://level_selector/level_selector.tscn");
        }
        else
        {
            LevelManager.Instance.EmitLevelExit(true);
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
            // Recursively collect mover that is being pushed by this mover
            Vector2I frontPos = mover.GridPosition + direction;
            Mover frontMover = GetMover(frontPos);
            if (frontMover != null && frontMover != mover && frontMover != this)
            {
                CollectMovingGroup(frontMover, direction, collected);
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
